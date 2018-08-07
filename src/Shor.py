#! /usr/bin/python
#-* coding: utf-8 -*
import multiprocessing
import time
import os
import sys
import time
global isSilent

global processSetFlag
global processNum
global isParallel

def Shor(arg):
    # print(str(type(arg).__name__))
    os.popen('dotnet run ' + str(arg) + ' 1>./ShorResultDir/' + str(arg) + '.result')

def ShorWrapper(args):
    import re

    flag = True
    res = []
    while flag:
        flag = False
        time.sleep(3)
        for arg in args:
            flag = True
            ar = []
            Shor(str(arg))
            line = ''
            with open('./ShorResultDir/'+str(arg)+'.result', 'r') as f:
                line = (f.readlines())[-1]
            line = line.replace('\n','')
            elems = re.split(r'[x=]', line)
            # print(elems)
            for elem in elems:
                ar.append(elem)
            res.append(ar)
            args.remove(arg)
    return res

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
        