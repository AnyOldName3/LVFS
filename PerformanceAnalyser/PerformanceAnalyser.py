#!/usr/bin/python

import os
import subprocess
import sys
import time


if len(sys.argv) != 5:
	print("Usage: PerformanceAnalyser.py <maxLayers> <numFiles> <numSamples> <path to LayeredDirectoryMirror.exe>")
	sys.exit()

maxLayers = int(sys.argv[1])
numFiles = int(sys.argv[2])
numSamples = int(sys.argv[3])

ldm = sys.argv[4]

dirPrefix = "folder"
filePrefix = "file"
oneWayName = "oneway"
virtualDrive = "x:"

results = open("results.csv", 'w')
results.write("Access times for " + str(numFiles) + " files:\n")
results.write("Layer count")
for i in range(1, numSamples + 1):
	results.write(", sample " + str(i))
results.write("\n")
results.flush()
os.fsync(results.fileno())

if not os.path.exists(dirPrefix + str(0)):
	os.makedirs(dirPrefix + str(0))

if not os.path.exists(oneWayName):
	os.makedirs(oneWayName)

for i in range(0, numFiles):
	if not os.path.exists(filePrefix + str(i)):
		open(os.path.join(dirPrefix + str(0), filePrefix + str(i)), "w+").close()

for i in range(0, maxLayers):
	if not os.path.exists(dirPrefix + str(i)):
		os.makedirs(dirPrefix + str(i))
		
	lvfsCmd = ldm + " " + virtualDrive
	for j in range(0, i + 1):
		lvfsCmd += " " + dirPrefix + str(j)
	lvfsCmd += " -o=" + oneWayName

	results.write(str(i + 1))

	for j in range(0, numSamples):
		# mount LVFS
		lvfs = subprocess.Popen(lvfsCmd)#, creationflags=subprocess.CREATE_NEW_CONSOLE)
		time.sleep(1)
		#start timer
		start = time.perf_counter()
		for k in range(0, numFiles):
			os.path.getatime(os.path.join(virtualDrive, os.sep, filePrefix + str(k)))
		#save timer
		timer = time.perf_counter() - start
		results.write("," + str(timer))
		# unmount LVFS
		subprocess.Popen("C:\\Program Files\\Dokan\\DokanLibrary-1.0.1\\Dokanctl.exe /u " + virtualDrive)
		lvfs.wait()
		time.sleep(1)

	results.write("\n")
	results.flush()
	os.fsync(results.fileno())

results.close()