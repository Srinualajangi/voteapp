# =============================================================================
# Vote Service Dockerfile (Python Flask)
# =============================================================================
# This Dockerfile uses MULTI-STAGE BUILD (DevSecOps best practice)
# Stage 1: Install dependencies in a builder image
# Stage 2: Copy only what's needed to a smaller runtime image
# Result: Smaller image size, fewer vulnerabilities

# Stage 1: Builder
FROM python:3.11-slim AS builder

WORKDIR /app

# Install dependencies
COPY requirements.txt .
RUN pip install --no-cache-dir --user -r requirements.txt

# Stage 2: Runtime
FROM python:3.11-slim

# Security: Run as non-root user (DevSecOps best practice)
RUN useradd --create-home --shell /bin/bash appuser

WORKDIR /app

# Copy installed packages from builder
COPY --from=builder /root/.local /home/appuser/.local

# Copy application code
COPY . .

# Update PATH to include user packages
ENV PATH=/home/appuser/.local/bin:$PATH

# Switch to non-root user
USER appuser

# Expose port
EXPOSE 8080

# Health check for Kubernetes
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD python -c "import urllib.request; urllib.request.urlopen('http://localhost:8080/health')" || exit 1

# Run with gunicorn (production-ready WSGI server)
CMD ["gunicorn", "-b", "0.0.0.0:8080", "-w", "4", "app:app"]
