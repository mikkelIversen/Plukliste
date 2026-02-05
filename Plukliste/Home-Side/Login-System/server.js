const express = require('express');
const path = require('path');
const fs = require('fs');
const session = require('express-session');
const bcrypt = require('bcryptjs');

const app = express();

const port = 3000;

app.set('view engine', 'ejs');
app.set('views', path.join(__dirname, '/'));

app.use(
  session({
    secret: 'dev-secret',
    resave: false,
    saveUninitialized: false,
    cookie: { maxAge: 1000 * 60 * 60 },
  })
);

app.get('/', (req, res) => {
  res.render('frontside', { title: 'Login System' });
});

app.get('/signup', (req, res) => {
  res.render('signup', { title: 'Sign Up' });
});

app.get('/login', (req, res) => {
  if (!req.session.user) return res.redirect('/');
  res.render('login', { title: 'Logget ind', username: req.session.user.username });
});

app.post('/login', (req, res) => {
  const { username, password } = req.body || {};
  try {
    const jsonPath = path.join(__dirname, 'user', 'user.json');
    const raw = fs.readFileSync(jsonPath, 'utf8');
    const data = JSON.parse(raw);
    const list = Array.isArray(data.userdata) ? data.userdata : (data.userdata ? [data.userdata] : []);

    const found = list.find(u => u.username === username);
    if (found && bcrypt.compareSync(password, found.password)) {
      req.session.user = { username: found.username };
      return res.redirect('/login');
    }
    return res.status(401).render('frontside', { title: 'Login System', alert: 'Ugyldigt brugernavn eller adgangskode' });
  } catch (err) {
    console.error('Fejl ved læsning af Databasen:', err);
    return res.status(500).send('Server fejl');
  }
});

app.post('/logout', (req, res) => {
  req.session.destroy(() => {
    res.redirect('/');
  });
});

app.post('/signup', (req, res) => {
  const { username, password, confirmPassword } = req.body || {};
  try {
    if (!username || !password || !confirmPassword) {
      return res.status(400).render('signup', { title: 'Sign Up', alert: 'Udfyld alle felter' });
    }
    if (password !== confirmPassword) {
      return res.status(400).render('signup', { title: 'Sign Up', alert: 'Adgangskode og Gentag adgangskode skal være ens' });
    }

    const jsonPath = path.join(__dirname, 'user', 'user.json');
    let data = { userdata: [] };
    if (fs.existsSync(jsonPath)) {
      const raw = fs.readFileSync(jsonPath, 'utf8');
      data = JSON.parse(raw);
    }
    if (!Array.isArray(data.userdata)) {
      data.userdata = data.userdata ? [data.userdata] : [];
    }

    const exists = data.userdata.some(u => u.username === username);
    if (exists) {
      return res.status(400).render('signup', { title: 'Sign Up', alert: 'Brugernavnet er allerede i brug' });
    }

    const hashed = bcrypt.hashSync(password, 10);
    data.userdata.push({ username, password: hashed });
    fs.writeFileSync(jsonPath, JSON.stringify(data, null, 2), 'utf8');

    return res.status(201).render('frontside', { title: 'Login System', alert: 'Bruger oprettet. Log ind.' });
  } catch (err) {
    console.error('Fejl ved skrivning til Database:', err);
    return res.status(500).render('signup', { title: 'Sign Up', alert: 'Server fejl' });
  }
});

app.listen(port, () => {
  console.log(`Server is running on http://localhost:${port}`);
});