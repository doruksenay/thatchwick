import urllib.request, os
jobs = {
 "celtic_knot": "b20602e5-5b91-4754-be13-69f4dbdbc44b",
 "woad_spiral": "a9c8d84e-875a-4859-b34e-d39c499bcc10",
 "rune": "bd369bcd-20e5-4db1-a1df-fa0cf5fa45f2",
 "cross": "fb7f0dae-4ffb-4a70-ab62-dc96a34e9818",
 "bird": "6412a2de-1a61-48da-8ea8-42b1b99f8363",
 "snake": "ce0f5c8c-694b-471c-a593-88baf3b6b9bd",
 "wheat_ear": "8dfe5f15-e365-429b-8741-90b9fc98e31e",
 "heraldic_crest": "04190242-cca6-492e-a2c1-049c80ae435e",
}
os.makedirs("art/results/021q-tmp", exist_ok=True)
for n, j in jobs.items():
    d = urllib.request.urlopen("https://api.pixellab.ai/mcp/images/%s/download" % j).read()
    open("art/results/021q-tmp/portrait_layer_tattoo_%s_neck_64.png" % n, "wb").write(d)
    print(n, len(d))
