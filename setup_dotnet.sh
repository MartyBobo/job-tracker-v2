#!/bin/bash
# Install .NET 8 SDK on Ubuntu 24.04

# Update packages
sudo apt-get update

# Install dependencies
sudo apt-get install -y wget apt-transport-https software-properties-common

# Add Microsoft package signing key and repository
wget -q https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# Install .NET SDK 8.0
sudo apt-get update
sudo apt-get install -y dotnet-sdk-8.0

# Verify installation
dotnet --version