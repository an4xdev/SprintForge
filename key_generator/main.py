import base64
import os

key = base64.b64encode(os.urandom(64))
print(key.decode())
