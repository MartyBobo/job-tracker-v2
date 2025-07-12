#!/bin/bash
# Install .NET 8 SDK for current user (no sudo required)

echo "Installing .NET 8 SDK for current user..."

# Download the install script
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod +x dotnet-install.sh

# Install .NET 8 SDK to user's home directory
./dotnet-install.sh --channel 8.0 --install-dir ~/.dotnet

# Add to PATH for current session
export PATH="$HOME/.dotnet:$PATH"

# Add to bashrc for permanent installation
echo 'export PATH="$HOME/.dotnet:$PATH"' >> ~/.bashrc

# Clean up
rm dotnet-install.sh

# Verify installation
~/.dotnet/dotnet --version

echo ""
echo "✅ .NET 8 SDK installed successfully!"
echo "📝 Note: Run 'source ~/.bashrc' or restart your terminal to use 'dotnet' command directly"
echo "🚀 For now, use '~/.dotnet/dotnet' instead of 'dotnet'"