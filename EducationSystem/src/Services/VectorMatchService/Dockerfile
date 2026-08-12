# Dockerfile for VectorMatchService (Python 3.10 / FastAPI / Sentence-BERT / FAISS)
FROM python:3.10-slim

WORKDIR /app

# Prevent Python from writing pyc files to disc and enable unbuffered output logging
ENV PYTHONDONTWRITEBYTECODE=1 \
    PYTHONUNBUFFERED=1

# Install system build dependencies, OpenBLAS, unixODBC, and Microsoft ODBC Driver 18 for SQL Server
RUN apt-get update && apt-get install -y --no-install-recommends \
    g++ \
    libopenblas-dev \
    python3-dev \
    unixodbc-dev \
    curl \
    gnupg2 \
    ca-certificates \
    && curl -fsSL https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor -o /usr/share/keyrings/microsoft-prod.gpg \
    && curl -fsSL https://packages.microsoft.com/config/debian/12/prod.list | tee /etc/apt/sources.list.d/mssql-release.list \
    && apt-get update \
    && ACCEPT_EULA=Y apt-get install -y --no-install-recommends msodbcsql18 \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

# Install Python package dependencies
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# Warm-cache Sentence-BERT model during image build phase for zero-download offline runtime
RUN python -c "from sentence_transformers import SentenceTransformer; SentenceTransformer('multi-qa-MiniLM-L6-cos-v1')"

# Copy service source code
COPY . .

# Expose service port
EXPOSE 5006

# Run FastAPI server
CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "5006"]
