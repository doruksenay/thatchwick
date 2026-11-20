"""İnceleme sayfası: adayları dinle, seç, Kabul/Ret/Yeniden üret (Project 45 bölüm 6, 45a bölüm 4).

Kullanım: python tools/audio/review.py   (tarayıcıda http://localhost:8765 açılır)
Kararlar iş dosyasına yazılır. Sonra: python tools/audio/promote.py --git
"""
from __future__ import annotations

import json
import mimetypes
import sys
import threading
import webbrowser
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path
from urllib.parse import unquote, urlparse

from common import ROOT, WORK, all_jobs, family_of, freq_counts, job_by_id, now_iso, read_json, BANK_FILE

PORT = 8765
HTML = (Path(__file__).parent / "review.html")


def family_refs() -> dict[str, list[str]]:
    """Aile başına referans klipleri: kabul edilmiş pilot işinin oyundaki dosyaları."""
    bank = read_json(BANK_FILE, {"entries": []})
    by_key = {e["key"]: e for e in bank["entries"]}
    refs = {}
    for j in all_jobs():
        if j.get("pilot") and j.get("status") == "done":
            e = by_key.get(str(j.get("key")))
            if not e:
                continue
            clips = e.get("clips") or [c for L in e.get("layers", []) for c in L["clips"]]
            refs.setdefault(str(j.get("family")), [])
            refs[str(j.get("family"))] += ["/res/" + c + ".ogg" for c in clips[:3]]
    return refs


def jobs_payload() -> list[dict]:
    out = []
    refs = family_refs()
    for j in all_jobs():
        if j.get("status") != "review":
            continue
        rep = read_json(WORK / j.id / "report.json", None)
        if rep is None:
            continue
        fam = family_of(j)
        _, keep = freq_counts(j)
        out.append({
            "id": j.id, "title": j.get("title", ""), "kind": j.get("kind"), "family": fam["name"],
            "freq": j.get("freq", "-"), "keep": keep, "loop": bool(j.get("loop", False)),
            "layered": len(j.get("layers") or []) > 1, "pilot": bool(j.get("pilot", False)),
            "interval": fam.get("preview_interval_ms", 650), "pitchVar": fam.get("pitch_var", 0.05),
            "volVarDb": fam.get("vol_var_db", 2.0), "groups": rep["groups"], "refs": refs.get(fam["name"], []),
            "slice_misses": rep.get("slice_misses", []), "note": j.get("note", ""),
        })
    return out


def decide(data: dict) -> dict:
    j = job_by_id(str(data["job"]))
    if not j or j.get("status") != "review":
        return {"ok": False, "error": "iş incelemede değil"}
    act = data.get("action")
    if act == "accept":
        pick = data.get("pick") or []
        if not pick and j.get("kind") == "voice_syllables":
            pick = "all_passed"  # hece bankası: hece başına kapıdan geçen ilk kayıt
        if not pick:
            return {"ok": False, "error": "en az bir aday seç"}
        j.meta.update({"status": "accepted", "pick": pick, "reviewed_at": now_iso()})
    elif act == "reject":
        j.meta.update({"status": "rejected", "reviewed_at": now_iso()})
    elif act == "regenerate":
        j.meta.update({"status": "ready", "regen": int(j.get("regen", 0)) + 1, "reviewed_at": now_iso()})
    else:
        return {"ok": False, "error": "bilinmeyen karar"}
    if data.get("note"):
        j.meta["note"] = str(data["note"])[:500]
    j.save()
    return {"ok": True, "status": j.meta["status"]}


class H(BaseHTTPRequestHandler):
    def log_message(self, *a):
        pass

    def _send(self, code: int, body: bytes, ctype: str):
        self.send_response(code)
        self.send_header("Content-Type", ctype)
        self.send_header("Content-Length", str(len(body)))
        self.send_header("Cache-Control", "no-store")
        self.end_headers()
        self.wfile.write(body)

    def _file(self, p: Path):
        try:
            p = p.resolve()
            if not (str(p).startswith(str(WORK.resolve())) or str(p).startswith(str((ROOT / "Assets").resolve()))):
                return self._send(403, b"yasak", "text/plain")
            if not p.exists():
                return self._send(404, b"yok", "text/plain")
            self._send(200, p.read_bytes(), mimetypes.guess_type(p.name)[0] or "audio/ogg")
        except Exception as e:
            self._send(500, str(e).encode(), "text/plain")

    def do_GET(self):
        path = unquote(urlparse(self.path).path)
        if path == "/":
            return self._send(200, HTML.read_bytes(), "text/html; charset=utf-8")
        if path == "/api/jobs":
            return self._send(200, json.dumps(jobs_payload(), ensure_ascii=False).encode(), "application/json")
        if path.startswith("/audio/"):
            _, _, job, name = path.split("/", 3)
            return self._file(WORK / job / "cand" / name)
        if path.startswith("/res/"):
            return self._file(ROOT / "Assets" / "Resources" / path[len("/res/"):])
        self._send(404, b"yok", "text/plain")

    def do_POST(self):
        if urlparse(self.path).path != "/api/decide":
            return self._send(404, b"yok", "text/plain")
        n = int(self.headers.get("Content-Length", 0))
        res = decide(json.loads(self.rfile.read(n) or b"{}"))
        self._send(200, json.dumps(res, ensure_ascii=False).encode(), "application/json")


def main() -> int:
    srv = ThreadingHTTPServer(("127.0.0.1", PORT), H)
    url = f"http://localhost:{PORT}"
    print(f"İnceleme sayfası: {url}  (kapatmak için Ctrl+C)")
    if "--no-browser" not in sys.argv:
        threading.Timer(0.6, lambda: webbrowser.open(url)).start()
    try:
        srv.serve_forever()
    except KeyboardInterrupt:
        pass
    print("\nKararlar iş dosyalarına yazıldı. Sıradaki adım: python tools\\audio\\promote.py --git")
    return 0


if __name__ == "__main__":
    sys.exit(main())
