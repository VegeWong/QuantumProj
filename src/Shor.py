#! /usr/bin/python
#-* coding: utf-8 -*
import multiprocessing
import time
import os
import sys

global isSilent

global processSetFlag
global processNum
global isParallel

def Shor(arg):
    os.popen('dotnet run ' + arg + ' 1>./ShorResultDir/' + arg + '.result')


def parsing(args):
    global isSilent
    global processSetFlag
    global processNum
    global isParallel

    argList = []
    for arg in args:
        print('Hello')
        print(arg)
        if (len(arg) > 2 and arg[0:1] == '--'):
            for i in range(2, len(arg)):
                if (arg[i] == 'v'):
                    isSilent = False
                if (arg[i] == 'p'):
                    isParallel = True
                    processSetFlag = True
        elif (arg.isdigit()):
            # print(arg)
            if (processSetFlag == True):
                processNum = int(arg)
                processSetFlag = False
            else:
                argList.append(arg)
        # else:
            # raise RuntimeError('Args should be integer')  
    return argList

def init():
    global isSilent
    global processSetFlag
    global processNum
    global isParallel
    
    isSilent = True
    processSetFlag = False
    processNum = multiprocessing.cpu_count()
    isParallel = False

if __name__ == '__main__':
    init()
    argList = parsing(sys.argv)
    processPool = multiprocessing.Pool(processNum)
    if (not os.path.exists('./ShorResultDir')):
        os.makedirs('./ShorResultDir')
    if (isParallel):
        for arg in argList:
            processPool.apply_async(Shor, arg)
        processPool.close()
        processPool.join()
        print("All threads are running, later check result in dirs")
    else:
        for arg in argList:
            Shor(arg)
        