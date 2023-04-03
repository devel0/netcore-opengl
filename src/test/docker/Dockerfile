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

RUN apt-get update && apt-get install -y libfontconfig-dev

# user mode

ARG USER_ID
ARG GROUP_ID

RUN groupadd -g ${GROUP_ID} user
RUN useradd -m -l -u ${USER_ID} -g user user
RUN apt-get install -y sudo strace
RUN echo "user ALL=(ALL) ALL" >> /etc/sudoers

# package reference cache

RUN mkdir /.refs
COPY refs/Program.cs /.refs
COPY refs/refs.csproj /.refs
RUN chown -R user.user /.refs

RUN su -c "dotnet build /.refs" user