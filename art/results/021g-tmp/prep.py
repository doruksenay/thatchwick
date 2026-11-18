import json, os, urllib.request, uuid
from PIL import Image, ImageDraw

D = os.path.dirname(os.path.abspath(__file__))
URL = "https://api.pixellab.ai/v2/uploads/eyJ1IjogImFhYzQ4MDc4LTkyOTItNDEwNi1iYjUyLTU3MDhlNmRmODZjNyIsICJleHAiOiAxNzk0OTkxMTAxfQ.0vykXI8nPqARGk9At0-GgQ2IX0bZimMJpif25ez9KVg"


def mk(name, long):
    m = Image.new("RGB", (64, 64), (0, 0, 0))
    d = ImageDraw.Draw(m)
    d.rectangle((12, 0, 51, 28), fill=(255, 255, 255))
    if long:
        d.rectangle((8, 29, 22, 58), fill=(255, 255, 255))
        d.rectangle((41, 29, 55, 58), fill=(255, 255, 255))
    p = os.path.join(D, name + ".png")
    m.save(p)
    return p


def up(path):
    b = uuid.uuid4().hex
    data = open(path, "rb").read()
    body = (f"--{b}\r\nContent-Disposition: form-data; name=\"file\"; filename=\"{os.path.basename(path)}\"\r\n"
            f"Content-Type: image/png\r\n\r\n").encode() + data + f"\r\n--{b}--\r\n".encode()
    r = urllib.request.Request(URL, body, {"Content-Type": f"multipart/form-data; boundary={b}"})
    return json.loads(urllib.request.urlopen(r).read())


base = "/home/runner/work/thatchwick/thatchwick/Assets/Art/Iso/_aday/portrait_base_oval_adult_64.png"
for p in (base, mk("mask_short", False), mk("mask_long", True)):
    print(os.path.basename(p), up(p))
