const express = require('express');
const cors = require('cors');
const jwt = require('jsonwebtoken');
const bcrypt = require('bcryptjs');
const app = express();

// Middleware
app.use(express.json());
app.use(cors({
  origin: ['http://localhost:3000', 'http://localhost:3100'],
  credentials: true
}));

// In-memory storage (replace with SQLite in production)
const users = new Map();
const applications = new Map();
const interviews = new Map();
const resumeTemplates = new Map();

// JWT Secret
const JWT_SECRET = 'your-256-bit-secret-key-here-change-in-production';

// Helper functions
const generateToken = (userId, email) => {
  return jwt.sign({ userId, email }, JWT_SECRET, { expiresIn: '15m' });
};

const generateRefreshToken = (userId) => {
  return jwt.sign({ userId, type: 'refresh' }, JWT_SECRET, { expiresIn: '7d' });
};

// Auth endpoints
app.post('/api/auth/register', async (req, res) => {
  const { email, password, firstName, lastName } = req.body;
  
  if (users.has(email)) {
    return res.status(409).json({ detail: 'Email already registered' });
  }

  const hashedPassword = await bcrypt.hash(password, 10);
  const userId = Date.now().toString();
  
  users.set(email, {
    id: userId,
    email,
    password: hashedPassword,
    firstName,
    lastName,
    createdAt: new Date().toISOString()
  });

  const accessToken = generateToken(userId, email);
  const refreshToken = generateRefreshToken(userId);

  res.status(201).json({
    accessToken,
    refreshToken,
    expiresIn: 900
  });
});

app.post('/api/auth/login', async (req, res) => {
  const { email, password } = req.body;
  
  const user = users.get(email);
  if (!user) {
    return res.status(401).json({ detail: 'Invalid credentials' });
  }

  const isValid = await bcrypt.compare(password, user.password);
  if (!isValid) {
    return res.status(401).json({ detail: 'Invalid credentials' });
  }

  const accessToken = generateToken(user.id, email);
  const refreshToken = generateRefreshToken(user.id);

  res.json({
    accessToken,
    refreshToken,
    expiresIn: 900
  });
});

app.post('/api/auth/refresh', (req, res) => {
  const { refreshToken } = req.body;
  
  try {
    const decoded = jwt.verify(refreshToken, JWT_SECRET);
    if (decoded.type !== 'refresh') {
      return res.status(401).json({ detail: 'Invalid refresh token' });
    }

    const user = Array.from(users.values()).find(u => u.id === decoded.userId);
    if (!user) {
      return res.status(401).json({ detail: 'User not found' });
    }

    const accessToken = generateToken(user.id, user.email);
    const newRefreshToken = generateRefreshToken(user.id);

    res.json({
      accessToken,
      refreshToken: newRefreshToken,
      expiresIn: 900
    });
  } catch (error) {
    res.status(401).json({ detail: 'Invalid refresh token' });
  }
});

// User endpoints
app.get('/api/users/me', (req, res) => {
  const authHeader = req.headers.authorization;
  if (!authHeader) {
    return res.status(401).json({ detail: 'No authorization header' });
  }

  const token = authHeader.replace('Bearer ', '');
  try {
    const decoded = jwt.verify(token, JWT_SECRET);
    const user = Array.from(users.values()).find(u => u.id === decoded.userId);
    
    if (!user) {
      return res.status(404).json({ detail: 'User not found' });
    }

    res.json({
      id: user.id,
      email: user.email,
      firstName: user.firstName,
      lastName: user.lastName
    });
  } catch (error) {
    res.status(401).json({ detail: 'Invalid token' });
  }
});

// Applications endpoints
app.get('/api/applications', (req, res) => {
  const apps = Array.from(applications.values());
  res.json(apps);
});

// Interviews endpoints
app.get('/api/interviews/upcoming', (req, res) => {
  const upcomingInterviews = Array.from(interviews.values())
    .filter(interview => new Date(interview.interviewDate) > new Date());
  res.json(upcomingInterviews);
});

// Health check
app.get('/health', (req, res) => {
  res.json({ status: 'healthy', timestamp: new Date().toISOString() });
});

const PORT = 5250;
app.listen(PORT, () => {
  console.log(`Mock backend server running on http://localhost:${PORT}`);
  console.log(`Swagger UI would be available at http://localhost:${PORT}/swagger`);
});