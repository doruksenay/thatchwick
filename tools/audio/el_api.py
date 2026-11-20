"""ElevenLabs REST istemcisi (yalnız standart kütüphane) ve kredisiz sahte (mock) istemci.

Uçlar (Project 45, bölüm 2):
  POST /v1/sound-generation                efekt ve ortam
  POST /v1/text-to-voice/design            ses tasarımı önizlemeleri
  POST /v1/text-to-voice                   önizlemeden kalıcı ses
  POST /v1/text-to-speech/{voice_id}       köy dili repliği / hece dizisi
  POST /v1/music                           müzik
  POST /v1/speech-to-text                  köy dili denetimi (gerçek kelime var mı)
  GET  /v1/user/subscription               kalan kredi
Model adları ve biçimler audio/style/families.json -> "api" altında; ElevenLabs değiştirirse oradan güncellenir.
"""
from __future__ import annotations

import base64
import io
import json
import math
import os
import random
import struct
import time
import urllib.error
import urllib.request
import uuid
import wave

BASE = "https://api.elevenlabs.io"


class ApiError(RuntimeError):
    def __init__(self, status: int, body: str):
        super().__init__(f"ElevenLabs {status}: {body[:400]}")
        self.status = status
        self.body = body


class ElevenLabs:
    def __init__(self, api_key: str | None = None, timeout: int = 300):
        self.key = api_key or os.environ.get("ELEVENLABS_API_KEY")
        if not self.key:
            raise RuntimeError("ELEVENLABS_API_KEY yok (.env ya da ortam değişkeni)")
        self.timeout = timeout

    # -- düşük seviye
    def _req(self, method: str, path: str, body: dict | None = None, raw: bytes | None = None,
             headers: dict | None = None, accept: str = "application/json", retries: int = 3):
        url = BASE + path
        h = {"xi-api-key": self.key, "Accept": accept}
        data = None
        if body is not None:
            data = json.dumps(body).encode("utf-8")
            h["Content-Type"] = "application/json"
        elif raw is not None:
            data = raw
        if headers:
            h.update(headers)
        for attempt in range(retries):
            req = urllib.request.Request(url, data=data, method=method, headers=h)
            try:
                with urllib.request.urlopen(req, timeout=self.timeout) as r:
                    return r.read(), r.headers.get("Content-Type", "")
            except urllib.error.HTTPError as e:
                txt = e.read().decode("utf-8", "replace")
                # 429 ve 5xx: bekleyip tekrar dene
                if e.code in (429, 500, 502, 503, 504) and attempt < retries - 1:
                    time.sleep(5 * (attempt + 1))
                    continue
                raise ApiError(e.code, txt) from None
            except urllib.error.URLError:
                if attempt < retries - 1:
                    time.sleep(5 * (attempt + 1))
                    continue
                raise

    # -- uçlar
    def subscription(self) -> dict:
        b, _ = self._req("GET", "/v1/user/subscription")
        return json.loads(b)

    def credits_left(self) -> int | None:
        s = self.subscription()
        used, limit = s.get("character_count"), s.get("character_limit")
        if used is None or limit is None:
            return None
        return int(limit) - int(used)

    def sound(self, text: str, model_id: str, duration: float | None, prompt_influence: float | None,
              loop: bool, output_format: str) -> bytes:
        body = {"text": text, "model_id": model_id}
        if duration:
            body["duration_seconds"] = round(float(duration), 2)
        if prompt_influence is not None:
            body["prompt_influence"] = float(prompt_influence)
        if loop:
            body["loop"] = True
        b, _ = self._req("POST", f"/v1/sound-generation?output_format={output_format}", body, accept="audio/*")
        return b

    def voice_design(self, description: str, model_id: str, sample_text: str | None) -> list[dict]:
        body = {"voice_description": description, "model_id": model_id}
        if sample_text:
            body["text"] = sample_text
        else:
            body["auto_generate_text"] = True
        b, _ = self._req("POST", "/v1/text-to-voice/design", body)
        data = json.loads(b)
        out = []
        for p in data.get("previews", []):
            out.append({"generated_voice_id": p["generated_voice_id"],
                        "audio": base64.b64decode(p["audio_base_64"])})
        return out

    def create_voice(self, name: str, description: str, generated_voice_id: str) -> str:
        body = {"voice_name": name, "voice_description": description, "generated_voice_id": generated_voice_id}
        b, _ = self._req("POST", "/v1/text-to-voice", body)
        return json.loads(b)["voice_id"]

    def tts(self, voice_id: str, text: str, model_id: str, output_format: str, seed: int | None) -> bytes:
        body = {"text": text, "model_id": model_id}
        if seed is not None:
            body["seed"] = int(seed) % 4294967295
        b, _ = self._req("POST", f"/v1/text-to-speech/{voice_id}?output_format={output_format}", body,
                         accept="audio/*")
        return b

    def music(self, prompt: str | None, plan: dict | None, length_ms: int | None, instrumental: bool,
              model_id: str | None, output_format: str) -> bytes:
        body: dict = {}
        if plan:
            body["composition_plan"] = plan
        else:
            body["prompt"] = prompt
            if length_ms:
                body["music_length_ms"] = int(length_ms)
            if instrumental:
                body["force_instrumental"] = True
        if model_id:
            body["model_id"] = model_id
        b, _ = self._req("POST", f"/v1/music?output_format={output_format}", body, accept="audio/*")
        return b

    def transcribe(self, audio: bytes, filename: str, model_id: str) -> str:
        boundary = uuid.uuid4().hex
        parts = []
        parts.append(f"--{boundary}\r\nContent-Disposition: form-data; name=\"model_id\"\r\n\r\n{model_id}\r\n".encode())
        parts.append(f"--{boundary}\r\nContent-Disposition: form-data; name=\"file\"; filename=\"{filename}\"\r\n"
                     f"Content-Type: application/octet-stream\r\n\r\n".encode() + audio + b"\r\n")
        parts.append(f"--{boundary}--\r\n".encode())
        b, _ = self._req("POST", "/v1/speech-to-text", raw=b"".join(parts),
                         headers={"Content-Type": f"multipart/form-data; boundary={boundary}"})
        return json.loads(b).get("text", "")


# ---------------------------------------------------------------- sahte istemci

def _wav_bytes(samples, sr: int = 44100, channels: int = 1) -> bytes:
    buf = io.BytesIO()
    with wave.open(buf, "wb") as w:
        w.setnchannels(channels)
        w.setsampwidth(2)
        w.setframerate(sr)
        frames = bytearray()
        for s in samples:
            v = max(-1.0, min(1.0, s))
            frames += struct.pack("<h", int(v * 32000))
        w.writeframes(bytes(frames))
    return buf.getvalue()


class MockElevenLabs:
    """Kredisiz deneme: gerçek API yerine sentetik ses üretir. Hattı uçtan uca sınamak için.

    Çıktılar WAV'dır; son işlem (ffmpeg) her biçimi okuduğu için hat aynı çalışır.
    """

    def __init__(self, credits: int = 30000):
        self._credits = credits

    def credits_left(self) -> int:
        return self._credits

    def _spend(self, n: int):
        self._credits -= int(n)

    def _seed(self, *parts) -> random.Random:
        return random.Random(hash(tuple(str(p) for p in parts)) & 0xFFFFFFFF)

    def sound(self, text, model_id, duration, prompt_influence, loop, output_format) -> bytes:
        dur = float(duration or 1.5)
        self._spend(dur * 40)
        r = self._seed(text, duration, time.time_ns())
        sr = 44100
        n = int(dur * sr)
        f0 = r.uniform(80, 900)
        decay = r.uniform(3, 18) if not loop else 0.0
        out = []
        lead = int(r.uniform(0.02, 0.12) * sr)  # baştaki sessizlik (kırpma sınansın)
        for i in range(n):
            t = i / sr
            env = math.exp(-decay * t) if decay else 0.6 + 0.2 * math.sin(2 * math.pi * 0.3 * t)
            v = 0.0 if i < lead else env * (0.5 * math.sin(2 * math.pi * f0 * t) + 0.35 * (r.random() * 2 - 1))
            out.append(v * 0.7)
        return _wav_bytes(out, sr)

    def voice_design(self, description, model_id, sample_text):
        self._spend(300)
        res = []
        for k in range(3):
            res.append({"generated_voice_id": f"mockgen_{uuid.uuid4().hex[:8]}",
                        "audio": self.sound(description + str(k), None, 2.5, None, False, "")})
        return res

    def create_voice(self, name, description, generated_voice_id) -> str:
        return "mockvoice_" + generated_voice_id[-8:]

    def tts(self, voice_id, text, model_id, output_format, seed) -> bytes:
        self._spend(len(text))
        r = self._seed(voice_id, text, seed)
        sr = 44100
        out = []
        base = r.uniform(110, 260)
        syllables = [s for s in text.replace("...", " ").replace(".", " ").split() if s]
        for s in syllables:
            d = 0.12 + 0.03 * len(s)
            f = base * r.uniform(0.9, 1.25)
            for i in range(int(d * sr)):
                t = i / sr
                env = math.sin(math.pi * t / d)
                out.append(0.5 * env * math.sin(2 * math.pi * f * t) * (1 + 0.3 * math.sin(2 * math.pi * 5 * t)))
            out.extend([0.0] * int(0.18 * sr))  # heceler arası sessizlik (dilimleme sınansın)
        return _wav_bytes(out or [0.0] * sr, sr)

    def music(self, prompt, plan, length_ms, instrumental, model_id, output_format) -> bytes:
        dur = (length_ms or 30000) / 1000.0
        self._spend(dur / 60 * 900)
        r = self._seed(prompt, time.time_ns())
        sr = 22050
        notes = [r.choice([220, 247, 262, 294, 330, 349, 392]) for _ in range(int(dur * 2) + 1)]
        out = []
        for i in range(int(dur * sr)):
            t = i / sr
            f = notes[int(t * 2)]
            out.append(0.3 * math.sin(2 * math.pi * f * t) * (0.6 + 0.4 * math.sin(math.pi * (t * 2 % 1))))
        return _wav_bytes(out, sr)

    def transcribe(self, audio, filename, model_id) -> str:
        return "ba lo mi"


def client(mock: bool):
    return MockElevenLabs() if mock else ElevenLabs()
