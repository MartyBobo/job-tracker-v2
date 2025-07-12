#!/bin/bash
set -e

echo "🚀 Starting JobTrackerV2 development environment..."

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
  echo "❌ Docker is not running. Please start Docker Desktop."
  exit 1
fi

# Start services
docker-compose up -d

# Wait for services to be healthy
echo "⏳ Waiting for services to be ready..."
sleep 5

# Check health
if curl -f http://localhost:3000/api/healthz > /dev/null 2>&1; then
  echo "✅ All services are up and running!"
  echo "📱 Web app: http://localhost:3000"
  echo "💾 MinIO console: http://localhost:9001 (minio/minio123)"
  echo "📧 MailHog: http://localhost:8025"
else
  echo "❌ Services failed to start. Check logs with: docker-compose logs"
  exit 1
fi