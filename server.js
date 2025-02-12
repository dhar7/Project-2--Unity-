const express = require('express');
const cors = require('cors');
const app = express();
const port = 3000;

// Middleware fixes
app.use(cors());
app.use(express.json()); // Add JSON body parser

app.post('/data', (req, res) => {
  try {
    // Validate request structure
    if (!req.body?.keys) {
      return res.status(400).json({ error: "Missing 'keys' array" });
    }

    // Process keys safely
    const keys = req.body.keys;
    if (!Array.isArray(keys)) {
      return res.status(400).json({ error: "'keys' must be an array" });
    }

    keys.forEach(keyData => {
      console.log(`Key: ${keyData.key}, Time: ${keyData.timestamp}`);
    });

    res.json({ status: 'success', received: keys.length });
  } catch (error) {
    console.error("Server error:", error);
    res.status(500).json({ error: "Internal server error" });
  }
});

app.listen(port, () => {
  console.log(`Server running at http://localhost:${port}`);
});
