"""PixelLab yükleme URL'sine PNG gönderir: python upload_ref.py <url> <png>..."""
import sys
import urllib.request
import uuid

url = sys.argv[1]
for path in sys.argv[2:]:
    b = uuid.uuid4().hex
    data = open(path, "rb").read()
    head = ("--%s\r\nContent-Disposition: form-data; name=\"file\"; filename=\"%s\"\r\n"
            "Content-Type: image/png\r\n\r\n" % (b, path.split("/")[-1]))
    body = head.encode() + data + ("\r\n--%s--\r\n" % b).encode()
    req = urllib.request.Request(url, body, {"Content-Type": "multipart/form-data; boundary=" + b})
    print(path, urllib.request.urlopen(req).read().decode())
