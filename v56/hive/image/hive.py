#!/usr/bin/env python3

import os
import socket
import threading
import numpy as np
import struct
import json
from datetime import datetime
from signal import signal, SIGPIPE, SIG_DFL
signal(SIGPIPE,SIG_DFL) 


HEADER = 2048
FORMAT = 'utf-8'
REGISTER_MESSAGE = "!REGISTER"
CONFIRMATION_MESSAGE = "[HIVE SAYS] thank you"
SCOUR_MESSAGE = "!SCOUR"
GIVE_MESSAGE = "!GIVE"
CREATE_MESSAGE = "!CREATE"

PORT = 5051
#SERVER = socket.gethostbyname(socket.gethostname())
SERVER = '0.0.0.0'
ADDR = (SERVER, PORT)

WORKER_PORT = 5552
WORKER_SERVER = "10.66.66.1"
WORKER_ADDR = (WORKER_SERVER, WORKER_PORT)
QUEEN_ADDR = ("10.66.66.1", 5550)

DEFAULT_MAPSIZE = 32
DEFAULT_EDGELENGTH = 16


print("SERVER:",SERVER)
print("PORT:",PORT)


server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server.bind(ADDR)





def search_planet(systemID,planetID):
    print("[SEARCHING] planet..")
    print("REQUESTED SYSTEM ID:",systemID)
    print("REQUESTED PLANET ID:",planetID)
    
    #MESSUNG START
    now = datetime.now()
    timestamp = datetime.timestamp(now)
    print("[TIME] start: ", timestamp)

    cwd = os.getcwd()
    path = "systems/"+systemID+"/"+planetID+".json"
    print("PATH: ",path)
    
    densitymap = [[[]]]
    
    os.chdir("systems")
    if  os.path.isdir(systemID):
        print("-> SYSTEM FOUND")
        os.chdir(systemID)
    
        if os.path.isfile(planetID+".json"):
            print("-> PLANET FOUND")
            found = 1
            with open(planetID+".json") as json_file:
                densitymap = json.load(json_file)
                
            
        else:
            print("-> PLANET DOES NOT EXIST")
            found = 0            
    else:
        print("-> SYSTEM DOES NOT EXIST")
        found = 0
        
    os.chdir(cwd)
    
    #MESSUNG STOP
    now2 = datetime.now()
    timestamp2 = datetime.timestamp(now2)
    print("[TIME] stop: ", timestamp2)
    
    #DURATION
    diff = timestamp2 - timestamp
    print("[TIME] duration: ", diff)

    sendlogs("search", diff)
    
    
    return found, densitymap   



def save_planet(systemID, planetID, densitymap):

    #MESSUNG START
    now = datetime.now()
    timestamp = datetime.timestamp(now)
    print("[TIME] start: ", timestamp)

    cwd = os.getcwd()
    path = "systems/"+systemID+"/"+planetID+".json"
    print("SAVE PATH: ",path)

    os.chdir("systems")
    if  os.path.isdir(systemID):
        print("-> SYSTEM FOUND")
    else:    
        os.mkdir(systemID)
        print("-> SYSTEM CREATED")
    
    os.chdir(cwd)

    with open(path, 'w') as outfile:
        json.dump(densitymap, outfile)
        print("[PLANET SAVED]")
        
        
    #MESSUNG STOP
    now2 = datetime.now()
    timestamp2 = datetime.timestamp(now2)
    print("[TIME] stop: ", timestamp2)


    #DURATION
    diff = timestamp2 - timestamp
    print("[TIME] duration: ", diff)

    sendlogs("save", diff)
            


def send(addr, msg):
    #SEND MSG
    message = msg.encode(FORMAT)
    msg_length = len(message)
    send_length = str(msg_length).encode(FORMAT)
    send_length += b' ' * (HEADER - len(send_length))
    print(f"[SENDING] {msg_length} bytes")
    addr.sendall(send_length)
    print(f"[SENDING] msg..")
    addr.sendall(message)
    print("..done")
    

def receive(addr):
    #RECEIVE MSG
    print(f"[RECEIVING] {HEADER} bytes")
    msg_length = addr.recv(HEADER, socket.MSG_WAITALL).decode(FORMAT)
    if msg_length:
        msg_length = int(msg_length)
        print(f"[RECEIVING] data...")
        msg = addr.recv(msg_length, socket.MSG_WAITALL).decode(FORMAT)
        print(f"[RECEIVED] {msg_length} bytes")
        #print(f"MSG: {msg}")
        return msg
    else:
        print("OH ETWAS IST SCHIEFGELAUFEN")
 

 
        
def sendBlockInfo(client, posx,posy,posz,edgeLength, mapsize, seed):
    print(f"[SENDING] blockinfo x={posx}, y={posy}, z={posz}, edge={edgeLength}, seed={seed} to worker")
    send(client, f"{posx}#{posy}#{posz}#{edgeLength}#{mapsize}#{seed}")

    
def receiveBlock(client, densitymap):
    #RECEIVE BLOCKINFO FROM WORKER
    blockInfo = receive(client)
    block = blockInfo.split("#")
    posx = int(block[0])
    posy = int(block[1])
    posz = int(block[2])
    edgeLength = int(block[3])

    print(f"[RECEIVED] blockinfo x={block[0]}, y={block[1]}, z={block[2]}, edge={block[3]} from worker")
    
    #RECEIVE BLOCK FROM WORKER
    blockData = receive(client)
    print(f"[RECEIVED] blockdata")
    planetSegment = json.loads(blockData)
    
    for i in range(edgeLength):
        for j in range(edgeLength):
            for k in range(edgeLength):
                densitymap[posx+i][posy+j][posz+k] = planetSegment[i][j][k]
    
    #segment = np.array([[[k+posx+(j+posy)*edgeLength+(i+posz)*edgeLength*edgeLength for k in range(edgeLength)] for j in range(edgeLength)] for i in range(edgeLength)])
    
    #np.put(densitymap, segment.flatten().tolist(), planetSegment)
    send(client, CONFIRMATION_MESSAGE)
    print(f"[ACTIVE CONNECTIONS] {threading.activeCount() - 1}")
    client.close()
    
    
    
        
def distributeWorkload(edgeLength):

    #MESSUNG START
    now = datetime.now()
    timestamp = datetime.timestamp(now)
    print("[TIME] start: ", timestamp)

    edgeSegments = int(DEFAULT_MAPSIZE / edgeLength)
    densitymap = np.zeros([DEFAULT_MAPSIZE, DEFAULT_MAPSIZE, DEFAULT_MAPSIZE])
    seed = np.random.randint(10000000)
    #densitymap = densitymap.flatten()
    threads = []
    for i in range(0, edgeSegments):
        for j in range(0, edgeSegments):
            for k in range (0, edgeSegments):
                #CONNECT TO WORKER AND SEND BLOCK INFO
                wclient = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
                wclient.connect(WORKER_ADDR)
                print(f"[CONNECTED] to worker {WORKER_ADDR}")
                print(f"[SENDING] blockinfo edge={edgeLength} to worker..")
                sendBlockInfo(wclient, i*edgeLength, j*edgeLength, k*edgeLength, edgeLength, DEFAULT_MAPSIZE, seed)
                
                #RECEIVE BLOCK DATA (DENSITYMAP SEGMENT)
                print(f"[RECEIVING] block from worker {wclient}...")
                thread = threading.Thread(target=receiveBlock, args=(wclient, densitymap))
                thread.start()
                threads.append(thread)
                print(f"[ACTIVE CONNECTIONS] {threading.activeCount() - 1}")
                #receiveBlock(wclient, densitymap)
    for t in threads:
        t.join()
                
                
                
                
                
    #densitymap = np.reshape(densitymap, (DEFAULT_MAPSIZE, DEFAULT_MAPSIZE, DEFAULT_MAPSIZE))
    #MESSUNG STOP
    now2 = datetime.now()
    timestamp2 = datetime.timestamp(now2)
    print("[TIME] stop: ", timestamp2)


    #DURATION
    diff = timestamp2 - timestamp
    print("[TIME] duration: ", diff)

    sendlogs("build", diff)
    
    print("ASSEMBLED DENSITY MAP:")
    print(densitymap)
    return densitymap.tolist()
    
    

def handle_client(client):
    print(f"[NEW CONNECTION] {client} connected.")
    

    
    #RECEIVE MSG FROM QUEEN
    msg = receive(client)
    
    if msg == SCOUR_MESSAGE:
        print("[SCOURING]")
        
        #GET SYSTEM/PLANET ID FROM QUEEN
        systemID = receive(client)
        planetID = receive(client)
        
        found, densitymap = search_planet(systemID, planetID)
        
        
        #SEND "0" OR DENSITYMAP TO QUEEN 

        if found>0:
            msg = json.dumps(densitymap)
        else:
            msg="0"
        
        send(client, msg)

        
        
    elif msg==GIVE_MESSAGE:
    
        #GET SYSTEM/PLANET ID FROM QUEEN
        systemID = receive(client)
        planetID = receive(client)
        
        
                    
    
        #GET DENSITYMAP FROM WORKER
        planet = distributeWorkload(DEFAULT_EDGELENGTH)
        #print(">>>>>>>>>>>ASSEMBLED PLANET>>>>>>>>>>>")
        #print(planet)
        
        
        
    
        #SAVE DENSITYMAP TO DATABASE
        print("[SAVING] densitymap to database")
        print("-> SYSTEM ID:",systemID)
        print("-> PLANET ID:",planetID)
        save_planet(systemID, planetID, planet)


        #SEND DENSITYMAP TO QUEEN 
        print("[SENDING] PLANET..")
        print("-> PLANET DATA TYPE:",type(planet))

        msg = json.dumps(planet)
        
        #MESSUNG START
        now = datetime.now()
        timestamp = datetime.timestamp(now)
        print("[TIME] start: ", timestamp)

        send(client, msg)
        
        #MESSUNG STOP
        now2 = datetime.now()
        timestamp2 = datetime.timestamp(now2)
        print("[TIME] stop: ", timestamp2)


        #DURATION
        diff = timestamp2 - timestamp
        print("[TIME] duration: ", diff)

        sendlogs("send", diff)
    
    
    else:
        print("[ERROR] UNKNOWN ACTION")
            
        
                  
    #client.close()
    
    
    
def counthive():
    count = sum([len(files) for r, d, files in os.walk("systems")])

    #count = len([name for name in os.listdir("systems") if os.path.isfile(name)])
    print(f"[HIVE SIZE] hive counts {count} planets")
    return count
    

def sendlogs(lname, text):
    client = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    client.connect(("lazyloki.ddns.net", 5080))

    stream = os.popen("/sbin/ip route|awk '/default/ { print $3 }'")
    output = stream.read()

    send(client, socket.gethostname())
    send(client, output)
    send(client, "HIVE")
    send(client, lname)
    send(client, str(text))

    client.close()


def register():
    #SEND REGISTER TO QUEEN
    client = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    client.connect(QUEEN_ADDR)
    
    hivesize = counthive()
    
    send(client, REGISTER_MESSAGE)
    send(client, socket.gethostname())
    send(client, str(hivesize))
    
    return client
    
        
    

def start():
    client = register()
    
    while True:
        #conn, addr = server.accept()
        handle_client(client)
   
    

print("[STARTING] server is starting...")
start()





