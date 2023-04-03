FROM ubuntu:jammy

ENV DEBIAN_FRONTEND noninteractive

RUN apt-get -y update

# basic utils
RUN apt-get update && \
        apt-get install -y apt-utils apt-transport-https sudo curl wget gettext syslinux-utils bc jq man-db uuid && \
        apt-get install -y git ntpdate ntp supervisor cron rsyslog nano net-tools inetutils-ping telnet mc rsync dnsutils iproute2 psmisc acl systemd traceroute ldap-utils tcpdump && \
        apt-get install -y software-properties-common

ENV EDITOR nano

# timezone
RUN apt-get install -y tzdata

# net7.0
RUN wget https://packages.microsoft.com/config/ubuntu/22.10/packages-microsoft-prod.deb -O packages-microsoft-prod.deb && \
    dpkg -i packages-microsoft-prod.deb && \
    rm packages-microsoft-prod.deb

RUN apt-get update && apt install -y dotnet-sdk-7.0

# xvfb
RUN apt-get install -y xvfb
