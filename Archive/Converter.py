import os
import csv
from tkinter import filedialog

WORKING_DIR = os.path.dirname(os.path.realpath(__file__))

fle = filedialog.askopenfilename(filetypes = [('TranslateFile','*.csv')], initialdir = WORKING_DIR)

csv_file = open(fle, "r", encoding="ms932")
f = csv.reader(csv_file, delimiter=",", doublequote=True, lineterminator="\r\n", quotechar='"', skipinitialspace=True)

for row in f:
  print(row[1])