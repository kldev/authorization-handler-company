#!/bin/bash

# Authorization Demo Showcase - Start Script
# ============================================

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

echo "=========================================="
echo "  Authorization Demo Showcase"
echo "=========================================="
echo ""

# Parse arguments
BUILD_ARG=""
DETACH_ARG="-d"

while [[ $# -gt 0 ]]; do
    case $1 in
        --build)
            BUILD_ARG="--build"
            shift
            ;;
        --foreground|-f)
            DETACH_ARG=""
            shift
            ;;
        --clean)
            echo "Cleaning up volumes and containers..."
            docker-compose down -v --remove-orphans
            echo "Clean complete."
            exit 0
            ;;
        --help|-h)
            echo "Usage: ./start.sh [OPTIONS]"
            echo ""
            echo "Options:"
            echo "  --build       Force rebuild of all images"
            echo "  --foreground  Run in foreground (see logs)"
            echo "  --clean       Remove all containers and volumes"
            echo "  --help        Show this help"
            echo ""
            echo "After starting, access:"
            echo "  App (Swagger):  http://localhost:8080/swagger"
            echo "  App (ReDoc):    http://localhost:8080/docs"
            echo "  Grafana:        http://localhost:3000"
            echo ""
            exit 0
            ;;
        *)
            echo "Unknown option: $1"
            exit 1
            ;;
    esac
done

echo "Starting services..."
echo ""

docker-compose up $BUILD_ARG $DETACH_ARG

if [ -n "$DETACH_ARG" ]; then
    echo ""
    echo "=========================================="
    echo "  Services started successfully!"
    echo "=========================================="
    echo ""
    echo "Endpoints:"
    echo "  App (Swagger):  http://localhost:8080/swagger"
    echo "  App (ReDoc):    http://localhost:8080/docs"
    echo "  Grafana:        http://localhost:3000"
    echo ""
    echo "Infrastructure:"
    echo "  PostgreSQL:     localhost:5432  (demo/demo)"
    echo "  Loki:           http://localhost:3100"
    echo "  Alloy:          http://localhost:12345"
    echo ""
    echo "Commands:"
    echo "  View logs:      docker-compose logs -f"
    echo "  App logs only:  docker-compose logs -f auth-app"
    echo "  Stop services:  docker-compose down"
    echo "  Clean all:      ./start.sh --clean"
    echo ""
fi
