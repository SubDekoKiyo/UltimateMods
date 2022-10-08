# Adapted from https://github.com/niveshbirangal/discord-rpc
# MIT License

# Copyright (c) 2021 Daemon

# Permission is hereby granted, free of charge, to any person obtaining a copy
# of this software and associated documentation files (the "Software"), to deal
# in the Software without restriction, including without limitation the rights
# to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
# copies of the Software, and to permit persons to whom the Software is
# furnished to do so, subject to the following conditions:

# The above copyright notice and this permission notice shall be included in all
# copies or substantial portions of the Software.

# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
# IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
# FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
# AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
# LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
# OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
# SOFTWARE.

import RPC
import time
from time import mktime

client_id = '1027553796223672340'
rpc_obj = RPC.DiscordIpcClient.for_platform(client_id)  # Send the client ID to the rpc module
print("RPC Connection Successful.")

time.sleep(5)
start_time = mktime(time.localtime())
while True:
    activity = {
            "state": "MainMenu",
            "details": "AmongUsをプレイ中",
            "timestamps": {
                "start": start_time
            },
            "assets": {
                "large_text": "Playing on UltimateMods",
                "large_image": "icon"
            }
        }
    rpc_obj.set_activity(activity)
    time.sleep(900)
