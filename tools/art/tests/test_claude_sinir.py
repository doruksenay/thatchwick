import datetime
import os
import sys
import tempfile
import unittest

sys.path.insert(0, os.path.join(os.path.dirname(__file__), ".."))
import queue_ready  # noqa: E402


class ClaudeCooldownTests(unittest.TestCase):
    def setUp(self):
        self.tmp = tempfile.NamedTemporaryFile("w", suffix=".md", delete=False, encoding="utf-8")
        self.old = queue_ready.CLAUDE_LIMIT
        queue_ready.CLAUDE_LIMIT = self.tmp.name

    def tearDown(self):
        queue_ready.CLAUDE_LIMIT = self.old
        os.unlink(self.tmp.name)

    def _write(self, text):
        with open(self.tmp.name, "w", encoding="utf-8") as f:
            f.write(text)

    def test_recent_hit_blocks(self):
        self._write("---\nzaman: 2026-11-21T00:00:00Z\niş: 810\n---\n")
        now = datetime.datetime(2026, 11, 21, 0, 20, tzinfo=datetime.timezone.utc)
        self.assertAlmostEqual(queue_ready.claude_cooldown(now), 40, delta=0.1)

    def test_old_hit_is_ignored(self):
        self._write("---\nzaman: 2026-11-21T00:00:00Z\n---\n")
        now = datetime.datetime(2026, 11, 21, 2, 0, tzinfo=datetime.timezone.utc)
        self.assertEqual(queue_ready.claude_cooldown(now), 0)

    def test_missing_file(self):
        queue_ready.CLAUDE_LIMIT = self.tmp.name + ".yok"
        self.assertEqual(queue_ready.claude_cooldown(), 0)


if __name__ == "__main__":
    unittest.main()
