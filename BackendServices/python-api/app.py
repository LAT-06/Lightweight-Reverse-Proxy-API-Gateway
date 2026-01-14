from flask import Flask, jsonify
import socket
import platform
from datetime import datetime

app = Flask(__name__)


@app.route("/api/health")
def health_check():
    return jsonify(
        {
            "status": "healthy",
            "service": "python-api",
            "hostname": socket.gethostname(),
            "timestamp": datetime.utcnow().isoformat(),
        }
    )


@app.route("/api/data")
def data():
    return jsonify(
        {
            "message": "data from python api",
            "service": "python-api",
            "hostname": socket.gethostname(),
            "datetime": {"items": ["item1", "item2", "item3"], "count": 3},
            "timestamp": datetime.utcnow().isoformat(),
        }
    )


@app.route("/api/info")
def info():
    return jsonify(
        {
            "service": "python-api",
            "version": "1.0.0",
            "hostname": socket.gethostname(),
            "platform": platform.system(),
            "platform_version": platform.python_version(),
            "timestamp": datetime.utcnow().isoformat(),
        }
    )


if __name__ == "__main__":
    print("Python API Service starting on port 5000")
    print(f"Hostname: {socket.gethostname()}")
    app.run(host="0.0.0.0", port=5000)

