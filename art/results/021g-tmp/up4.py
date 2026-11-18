import json, os, urllib.request, uuid
D = os.path.dirname(os.path.abspath(__file__))
URL = "https://api.pixellab.ai/v2/uploads/eyJ1IjogImFhYzQ4MDc4LTkyOTItNDEwNi1iYjUyLTU3MDhlNmRmODZjNyIsICJleHAiOiAxNzk0OTkxMTAxfQ.0vykXI8nPqARGk9At0-GgQ2IX0bZimMJpif25ez9KVg"
for n in ("braid_single", "braids_two", "topknot", "receding"):
    p = os.path.join(D, f"portrait_layer_hair_{n}_64.png")
    b = uuid.uuid4().hex
    body = (f"--{b}\r\nContent-Disposition: form-data; name=\"file\"; filename=\"{n}.png\"\r\nContent-Type: image/png\r\n\r\n").encode() + open(p, "rb").read() + f"\r\n--{b}--\r\n".encode()
    r = urllib.request.Request(URL, body, {"Content-Type": f"multipart/form-data; boundary={b}"})
    print(n, json.loads(urllib.request.urlopen(r).read())["ref"])
