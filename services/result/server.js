/**
 * VoteApp - Result Service (Node.js Express)
 * ===========================================
 * This is the dashboard that shows voting results.
 * It connects to PostgreSQL to read the aggregated vote counts.
 */
const express = require('express');
const { Pool } = require('pg');
const path = require('path');

const app = express();
const port = process.env.PORT || 8080;

// PostgreSQL configuration from environment variables
const pool = new Pool({
    host: process.env.POSTGRES_HOST || 'db',
    port: process.env.POSTGRES_PORT || 5432,
    database: process.env.POSTGRES_DB || 'postgres',
    user: process.env.POSTGRES_USER || 'postgres',
    password: process.env.POSTGRES_PASSWORD || 'postgres',
});

// Serve static files
app.use(express.static(path.join(__dirname, 'public')));

// API endpoint to get vote results
app.get('/api/results', async (req, res) => {
    try {
        const result = await pool.query(
            'SELECT vote, COUNT(id) as count FROM votes GROUP BY vote'
        );
        
        let votes = { a: 0, b: 0 };
        result.rows.forEach(row => {
            votes[row.vote] = parseInt(row.count);
        });
        
        res.json({
            a: votes.a,
            b: votes.b,
            total: votes.a + votes.b
        });
    } catch (err) {
        console.error('Database error:', err);
        // Return zeros if DB not ready
        res.json({ a: 0, b: 0, total: 0 });
    }
});

// Health check endpoint for Kubernetes
app.get('/health', (req, res) => {
    res.status(200).send('OK');
});

// Main page
app.get('/', (req, res) => {
    res.sendFile(path.join(__dirname, 'public', 'index.html'));
});

app.listen(port, '0.0.0.0', () => {
    console.log(`Result service listening on port ${port}`);
});
