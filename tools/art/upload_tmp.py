import sys, urllib.request, uuid
url, path = sys.argv[1], sys.argv[2]
d = open(path, "rb").read()
b = uuid.uuid4().hex
head = "--%s\r\nContent-Disposition: form-data; name=\"file\"; filename=\"base.png\"\r\nContent-Type: image/png\r\n\r\n" % b
body = head.encode() + d + ("\r\n--%s--\r\n" % b).encode()
r = urllib.request.Request(url, body, {"Content-Type": "multipart/form-data; boundary=" + b})
print(urllib.request.urlopen(r).read())
