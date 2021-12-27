FROM alpine:latest
MAINTAINER DCVDS

RUN apk --no-cache upgrade
    
ENV PYTHONUNBUFFERED=1
RUN apk add --update --no-cache python3 && ln -sf python3 /usr/bin/python
RUN apk add --update --no-cache python3-dev
RUN apk add --no-cache gcc \
                       python2-dev \
                       libc-dev \
    && rm -rf /var/cache/apk
RUN python3 -m ensurepip
RUN python3 -m pip install --upgrade pip
RUN pip3 install --no-cache --upgrade pip setuptools
#RUN pip install websockets
RUN pip3 install perlin_noise #&& pip3 install matplotlib 
RUN pip3 install noise
RUN apk add --update --no-cache py3-numpy
ENV PYTHONPATH=/usr/lib/python3.8/site-packages


EXPOSE 5052

WORKDIR /home
COPY command.sh /home
COPY worker.py /home

ENTRYPOINT ["/bin/sh", "/home/command.sh"]
