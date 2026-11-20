"""Ses hattı ortak yardımcıları: yollar, .env, iş dosyası okuma/yazma, aile ayarları, git.

Belge: Project 45 (hat), 45a (doğallık ve tekrar).
"""
from __future__ import annotations

import json
import os
import socket
import subprocess
import sys
from datetime import datetime, timezone
from pathlib import Path

import yaml

ROOT = Path(__file__).resolve().parents[2]
QUEUE = ROOT / "audio" / "queue"
STYLE = ROOT / "audio" / "style"
VOICES_FILE = ROOT / "audio" / "voices.json"
BANK_FILE = ROOT / "Assets" / "Resources" / "Audio" / "soundbank.json"   # tek kaynak; Unity de bunu okur
MANIFEST_FILE = ROOT / "audio" / "manifest.json"
BUDGET_FILE = ROOT / "audio" / "budget.md"
UNITY_AUDIO = ROOT / "Assets" / "Resources" / "Audio"
WORK = ROOT / ".audio_work"          # adaylar (git dışı)

STATUSES = ("ready", "claimed", "review", "accepted", "rejected", "blocked", "done")
RUNNABLE_APPROVALS = ("auto", "kullanici", "kullanıcı", "user")
DONE_STATES = ("accepted", "done")


def now_iso() -> str:
    return datetime.now(timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ")


def load_env() -> None:
    """Repo kökündeki .env dosyasını ortam değişkenlerine yükler (varsa)."""
    env = ROOT / ".env"
    if not env.exists():
        return
    for line in env.read_text(encoding="utf-8-sig").splitlines():
        line = line.strip()
        if not line or line.startswith("#") or "=" not in line:
            continue
        k, v = line.split("=", 1)
        os.environ.setdefault(k.strip(), v.strip().strip('"').strip("'"))


def masters_dir() -> Path:
    """Kayıpsız ana kopyaların klasörü. Varsayılan: masaüstü ses\\masters."""
    p = os.environ.get("AUDIO_MASTERS")
    if p:
        return Path(p)
    home = Path.home()
    for cand in (home / "OneDrive" / "Desktop", home / "Desktop"):
        if cand.exists():
            return cand / "ses" / "masters"
    return ROOT / ".audio_masters"


def review_copy_dir() -> Path | None:
    """Adayların kopyalandığı masaüstü klasörü (art klasörünün eşi)."""
    p = os.environ.get("AUDIO_REVIEW_COPY")
    if p:
        return Path(p)
    home = Path.home()
    for cand in (home / "OneDrive" / "Desktop", home / "Desktop"):
        if cand.exists():
            return cand / "ses" / "adaylar"
    return None


# ---------------------------------------------------------------- iş dosyaları

class Job:
    def __init__(self, path: Path, meta: dict, body: str):
        self.path = path
        self.meta = meta
        self.body = body

    @property
    def id(self) -> str:
        return str(self.meta.get("id", self.path.stem))

    def get(self, key, default=None):
        return self.meta.get(key, default)

    def save(self) -> None:
        write_job(self.path, self.meta, self.body)

    def __repr__(self) -> str:
        return f"Job({self.id}, {self.meta.get('kind')}, {self.meta.get('status')})"


def parse_job(path: Path) -> Job:
    text = path.read_text(encoding="utf-8-sig")
    if not text.startswith("---"):
        raise ValueError(f"{path.name}: ön bilgi (---) yok")
    parts = text.split("---", 2)
    if len(parts) < 3:
        raise ValueError(f"{path.name}: ön bilgi kapanmamış")
    meta = yaml.safe_load(parts[1]) or {}
    body = parts[2].lstrip("\n")
    return Job(path, meta, body)


def write_job(path: Path, meta: dict, body: str) -> None:
    head = yaml.safe_dump(meta, allow_unicode=True, sort_keys=False, width=1000).strip()
    # BOM'suz UTF-8 (kuyruk betikleri BOM'u okuyamaz; bkz. yapilacaklar süreç notları)
    path.write_text(f"---\n{head}\n---\n{body}", encoding="utf-8", newline="\n")


def all_jobs() -> list[Job]:
    jobs = []
    for p in sorted(QUEUE.rglob("*.md")):
        if p.name.lower() == "readme.md":
            continue
        try:
            jobs.append(parse_job(p))
        except Exception as e:  # bozuk dosya kuyruğu durdurmasın
            print(f"[uyarı] {p.relative_to(ROOT)}: {e}", file=sys.stderr)
    return jobs


def job_by_id(job_id: str) -> Job | None:
    for j in all_jobs():
        if j.id == job_id:
            return j
    return None


def deps_satisfied(job: Job, by_id: dict[str, Job]) -> bool:
    for dep in job.get("depends_on") or []:
        d = by_id.get(str(dep))
        if d is None or d.get("status") not in DONE_STATES:
            return False
    return True


def ready_jobs(worker_kind: str = "yerel") -> list[Job]:
    jobs = all_jobs()
    by_id = {j.id: j for j in jobs}
    out = []
    for j in jobs:
        if j.get("status") != "ready":
            continue
        if str(j.get("approved_by", "pending")).lower() not in RUNNABLE_APPROVALS:
            continue
        w = str(j.get("worker", "any"))
        if w not in ("any", worker_kind):
            continue
        if not deps_satisfied(j, by_id):
            continue
        out.append(j)
    # pilotlar önce, sonra sıklık sınıfı (A > B > C), sonra kimlik
    freq_rank = {"A": 0, "B": 1, "C": 2}
    out.sort(key=lambda j: (0 if j.get("pilot") else 1, freq_rank.get(str(j.get("freq", "B")), 1), j.id))
    return out


# ---------------------------------------------------------------- stil

def load_families() -> dict:
    return json.loads((STYLE / "families.json").read_text(encoding="utf-8"))


def family_of(job: Job) -> dict:
    fams = load_families()
    name = str(job.get("family", ""))
    if name not in fams["families"]:
        raise KeyError(f"{job.id}: bilinmeyen aile '{name}' (audio/style/families.json)")
    fam = dict(fams["defaults"])
    fam.update(fams["families"][name])
    fam["name"] = name
    return fam


def freq_counts(job: Job) -> tuple[int, int]:
    """(üretilecek, tutulacak) sayısı: iş dosyası > sıklık sınıfı (45a bölüm 1)."""
    fams = load_families()
    cls = fams["freq_classes"].get(str(job.get("freq", "B")), fams["freq_classes"]["B"])
    gen = int(job.get("generate", cls["generate"]))
    keep = int(job.get("keep", cls["keep"]))
    return gen, keep


# ---------------------------------------------------------------- json yardımcıları

def read_json(path: Path, default):
    if path.exists():
        return json.loads(path.read_text(encoding="utf-8"))
    return default


def write_json(path: Path, data) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8", newline="\n")


# ---------------------------------------------------------------- git

def git(*args: str, check: bool = True) -> subprocess.CompletedProcess:
    return subprocess.run(["git", *args], cwd=ROOT, check=check, capture_output=True, text=True)


def git_commit_push(paths: list[str], message: str) -> bool:
    """Yalnız verilen yolları commit'ler; pull --rebase ve push yapar. Değişiklik yoksa False."""
    paths = [p for p in paths if (ROOT / p).exists()]
    if not paths:
        return False
    git("add", "--", *paths)
    if git("diff", "--cached", "--quiet", check=False).returncode == 0:
        return False
    git("commit", "-m", message)
    branch = git("rev-parse", "--abbrev-ref", "HEAD").stdout.strip()
    r = git("pull", "--rebase", "origin", branch, check=False)
    if r.returncode != 0:
        print(r.stdout + r.stderr, file=sys.stderr)
        git("rebase", "--abort", check=False)
        raise RuntimeError("git pull --rebase başarısız; elle bak")
    git("push", "origin", "HEAD")
    return True


def host() -> str:
    return socket.gethostname()
