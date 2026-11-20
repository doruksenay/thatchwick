import os
import sys
import unittest

sys.path.insert(0, os.path.join(os.path.dirname(__file__), ".."))
import notify  # noqa: E402


class NotifyTests(unittest.TestCase):
    def test_body_mentions_owner_and_job(self):
        jobs = sorted(n.split("-")[0] for n in os.listdir(notify.QUEUE) if n.endswith(".md") and n[0].isdigit())
        job = jobs[0]
        text = notify.body(job, "success", "o/r", "sahip")
        self.assertTrue(text.startswith(f"@sahip **{job}**"))
        self.assertIn("https://github.com/o/r/blob/main/art/queue/", text)

    def test_push_failure_is_reported(self):
        job = sorted(n.split("-")[0] for n in os.listdir(notify.QUEUE) if n.endswith(".md") and n[0].isdigit())[0]
        self.assertIn("push edilemedi (failure)", notify.body(job, "failure", "o/r", "sahip"))

    def test_claude_limit(self):
        job = sorted(n.split("-")[0] for n in os.listdir(notify.QUEUE) if n.endswith(".md") and n[0].isdigit())[0]
        self.assertIn("sıraya geri kondu", notify.body(job, "claude-limit", "o/r", "sahip"))

    def test_unknown_job_does_not_crash(self):
        self.assertIn("**yok-boyle-is**", notify.body("yok-boyle-is", "success", "o/r", "sahip"))


if __name__ == "__main__":
    unittest.main()
