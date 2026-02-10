"""
VoteApp - Vote Service (Python Flask)
======================================
This is the frontend where users cast their votes.
It connects to Redis to store votes temporarily.
"""
from flask import Flask, render_template, request, make_response, g
from redis import Redis
import os
import socket
import random
import json

# Get configuration from environment variables
REDIS_HOST = os.getenv('REDIS_HOST', 'redis')
REDIS_PORT = int(os.getenv('REDIS_SERVICE_PORT', 6379))

# Vote options
option_a = os.getenv('OPTION_A', "Cats")
option_b = os.getenv('OPTION_B', "Dogs")

hostname = socket.gethostname()

app = Flask(__name__)

def get_redis():
    """Connect to Redis (in-memory cache for storing votes)"""
    if not hasattr(g, 'redis'):
        g.redis = Redis(host=REDIS_HOST, port=REDIS_PORT, db=0, socket_timeout=5)
    return g.redis

@app.route("/", methods=['POST', 'GET'])
def hello():
    """Main page - Show voting options and handle votes"""
    voter_id = request.cookies.get('voter_id')
    if not voter_id:
        voter_id = hex(random.getrandbits(64))[2:-1]

    vote = None

    if request.method == 'POST':
        redis = get_redis()
        vote = request.form['vote']
        app.logger.info('Received vote for %s', vote)
        
        # Store vote in Redis as JSON
        data = json.dumps({'voter_id': voter_id, 'vote': vote}, separators=(',', ':'))
        redis.rpush('votes', data)

    resp = make_response(render_template(
        'index.html',
        option_a=option_a,
        option_b=option_b,
        hostname=hostname,
        vote=vote,
    ))
    resp.set_cookie('voter_id', voter_id)
    return resp

@app.route("/health")
def health():
    """Health check endpoint for Kubernetes"""
    return "OK", 200

if __name__ == "__main__":
    app.run(host='0.0.0.0', port=8080, debug=True, threaded=True)
