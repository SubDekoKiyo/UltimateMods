import os

WORKING_DIR = os.path.dirname(os.path.realpath(__file__))

TRANSLATE_FILE = os.path.join(WORKING_DIR, "UltimateMods", "Resources", "Translate.json")
BUILD_NUM_DATA_FILE = os.path.join(WORKING_DIR, "BuildNumData.txt")

with open(TRANSLATE_FILE)as t:
    translate = t.readlines()

with open(BUILD_NUM_DATA_FILE, "r") as x:
    oldNum = x.read()

num = int(oldNum) + 1

translate.insert(1,'    "BuildNum": {\n        "0": "' + str(num) + '"\n    },\n')

with open(TRANSLATE_FILE, "w") as y:
    y.writelines(translate)

with open(BUILD_NUM_DATA_FILE, "w", newline="\n") as z:
    z.write(str(num))

print('[Build Number] ' + str(num))