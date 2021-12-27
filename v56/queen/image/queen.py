#!/usr/bin/env python3

import socket
import threading
import struct
import json
import numpy
import os
from datetime import datetime
from signal import signal, SIGPIPE, SIG_DFL
signal(SIGPIPE,SIG_DFL) 



HEADER = 2048
FORMAT = 'utf-8'
DISCONNECT_MESSAGE = "!DISCONNECT"
REGISTER_MESSAGE = "!REGISTER"
CONFIRMATION_MESSAGE = "[QUEEN SAYS] thank you"
SCOUR_MESSAGE = "!SCOUR"
GIVE_MESSAGE = "!GIVE"

PORT = 5050
#SERVER = socket.gethostbyname(socket.gethostname())
ADDR = ('0.0.0.0', PORT)

HIVE_PORT = 5051
HIVE_SERVER = "hive"
HIVE_ADDR = (HIVE_SERVER, HIVE_PORT)



#print("SERVER:",SERVER)
#print("PORT:",PORT)


server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server.bind(ADDR)


hivelist = []




def getLowestCapacityHive():
    i = numpy.amin([entry[1] for entry in hivelist])
    minimum = [entry[0] for entry in hivelist if entry[1]==i]
    print(f"LOWEST HIVE CAPACITY: {minimum[0]}")
    return minimum[0]


def increaseHiveSize(hive):
    for h in hivelist:
        if h[0] == hive:
            h = (h[0], h[1] +1)
    


def scourHives(systemID, planetID):
    for h in hivelist:
        hive = h[0]

        print('Hive Content:')
        print(hive)
        
        #SCOUR HIVE FOR PLANET
        send(hive, SCOUR_MESSAGE)
        send(hive, systemID)
        send(hive, planetID)
        
        
        #GET DENSITY MAP FROM HIVE WHEN FOUND 
        msg = receive(hive)
        
        return msg
        
        

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
        print(f"MSG: {msg}")
        return msg        
    
    


def handle_client(conn, addr):
    print(f"[NEW CONNECTION] {addr} connected.")
    
    
    connected = True
    while connected:
        #RECEIVE MSG FROM CLIENT
        msg = receive(conn)
            
            
        if msg == DISCONNECT_MESSAGE:
            #DISCONNECT RECEIVED
            print(f"[RECEIVED] disconnect msg from {addr}")
            connected = False
            conn.close()
            
        elif msg == REGISTER_MESSAGE:
            #REGISTER RECEIVED
            print(f"[RECEIVED] register msg from hive {addr}")
            connected = False
            
            address = receive(conn)
            msg = receive(conn)   
            
            hivesize = int(msg)
            hivelist.append((conn, hivesize))
                
        else:
            #MESSUNG START
            now = datetime.now()
            timestamp = datetime.timestamp(now)
            print("[TIME] start: ", timestamp)
            
            
            #SYSTEM ID RECEIVED
            systemID = msg
            print(f"[RECEIVED] systemID {systemID} from client {addr}")
            
            
            #RECEIVE PLANET ID FROM CLIENT
            planetID = receive(conn)
            print(f"[RECEIVED] planetID {planetID} from client {addr}")
            
            
            #SCOUR HIVES
            r = scourHives(systemID, planetID)
            print(f"r = {r}")
            if r != "0" and r != None:
                print("PLANET FOUND")
                planet = json.loads(r)
            
            else:
                print("PLANET NOT FOUND")
                #GET HIVE WITH LOWEST CAPACITY
                freehive = getLowestCapacityHive()
                
                print(f"FREE HIVE:  {freehive}")

                #SEND GIVE MESSAGE TO HIVE
                send(freehive, GIVE_MESSAGE)
                
                #SEND SYSTEM/PLANET ID TO HIVE
                send(freehive, systemID)
                send(freehive, planetID)
                
                
                #GET DENSITY MAP FROM HIVE
                msg = receive(freehive)
                
                #INCREASE HIVE SIZE
                increaseHiveSize(freehive)
                 

                planet = json.loads(msg)
                
                
                
            #SEND DENSITYMAP TO CLIENT 
            msg = json.dumps(planet)
            
            send(conn, msg)
            
            #MESSUNG STOP
            now2 = datetime.now()
            timestamp2 = datetime.timestamp(now2)
            print("[TIME] stop: ", timestamp2)
            
            #DURATION
            diff = timestamp2 - timestamp
            print("[TIME] duration: ", diff)

            sendlogs("total", diff)
            
                
                
    #conn.close()
    print("[CONNECTION CLOSED]\n")
        
def sendlogs(lname, text):
    client = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    client.connect(("lazyloki.ddns.net", 5080))

    stream = os.popen("/sbin/ip route|awk '/default/ { print $3 }'")
    output = stream.read()

    send(client, socket.gethostname())
    send(client, output)
    send(client, "QUEEN")
    send(client, lname)
    send(client, str(text))

    client.close()

def start():
    server.listen()
    #print(f"[LISTENING] Server is listening on {SERVER}")
    while True:
        thread = threading.Thread(target=handle_client, args=(server.accept()))
        thread.start()
        print(f"[ACTIVE CONNECTIONS] {threading.activeCount() - 1}")
    
    

print("[STARTING] server is starting...")
start()





