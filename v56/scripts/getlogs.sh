#!/bin/sh
IP=$1
wget $IP:8000/queen-log.txt
wget $IP:8000/hive-search-log.txt
wget $IP:8000/hive-save-log.txt
wget $IP:8000/hive-build-log.txt
wget $IP:8000/hive-send-log.txt
wget $IP:8000/worker-create-log.txt
wget $IP:8000/worker-send-log.txt