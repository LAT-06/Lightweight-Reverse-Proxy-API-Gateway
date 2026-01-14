const express = require('express');
const os = require('os');

const app = express();
const PORT = process.env.PORT ||
    3000;

app.use(express.json());

app.get('/api/health', (req, res) => {
    res.json({
        status: 'healthy',
        service: 'nodejs-api',
        hostname: os.hostname(),
        timestamp: new
            Date().toISOString()
    });
});

app.get('/api/data', (req, res) => {
    res.json({
        message: 'Data from Node.js API',
        service: 'nodejs-api',
        hostname: os.hostname(),
        data: {
            items: ['item1', 'item2',
                'item3'],
            count: 3
        },
        timestamp: new
            Date().toISOString()
    });
});

app.get('/api/info', (req, res) => {
    res.json({
        service: 'nodejs-api',
        version: '1.0.0',
        hostname: os.hostname(),
        platform: os.platform(),
        arch: os.arch(),
        nodeVersion: process.version,
        uptime: process.uptime(),
        timestamp: new
            Date().toISOString()
    });
});

app.listen(PORT, () => {
    console.log(`Node.js API running on
    port ${PORT}`);
    console.log(`Hostname:
   ${os.hostname()}`);
});

