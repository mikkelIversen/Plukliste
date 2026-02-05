const express = require('express');
const path = require('path');
const fs = require('fs');
const session = require('express-session');
const bcrypt = require('bcryptjs');

const app = express();

const port = 3000;

app.set('view engine', 'ejs');
app.set('views', path.join(__dirname, '/'));

app.use(express.json());
app.use(express.urlencoded({ extended: true }));

app.use(
  session({
    secret: 'dev-secret', 
    resave: false,      
    saveUninitialized: false,
    cookie: { maxAge: 1000 * 60 * 60 }, 
  })
);

app.get('/', (req, res) => {
  res.render('Login_Page', { title: 'Login System' });
});

app.get('/signup', (req, res) => {
  res.render('signup_page', { title: 'Sign Up' });
});

app.get('/dashboard', (req, res) => {
  if (!req.session.user) return res.redirect('/');
  res.render('Test', { title: 'Logget ind', username: req.session.user.username });
});

app.post('/login', (req, res) => {
  const { username, password } = req.body || {};
  
  console.log('Login attempt:', { username, password: password ? '[HIDDEN]' : undefined });
  console.log('Full login request body:', req.body);
  
  try {
    const jsonPath = path.join(__dirname, 'user', 'user.json');
    const raw = fs.readFileSync(jsonPath, 'utf8');
    const data = JSON.parse(raw);
    console.log('User data from file:', data);
    
    const list = Array.isArray(data.userdata) ? data.userdata : (data.userdata ? [data.userdata] : []);
    console.log('User list:', list);

    const found = list.find(u => u.username === username);
    console.log('Found user:', found ? { username: found.username, hasPassword: !!found.password } : 'not found');
    
    if (found && bcrypt.compareSync(password, found.password)) {
      console.log('Password match successful');
      req.session.user = { username: found.username };
      return res.redirect('/dashboard');
    }
    console.log('Login failed - invalid credentials');
    return res.status(401).render('Login_Page', { title: 'Login System', alert: 'Ugyldigt brugernavn eller adgangskode' });
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
  
  console.log('Received signup data:', { username, password: password ? '[HIDDEN]' : undefined, confirmPassword: confirmPassword ? '[HIDDEN]' : undefined });
  console.log('Full request body:', req.body);
  
  try {
    if (!username || !password || !confirmPassword) {
      console.log('Missing fields detected');
      return res.status(400).render('signup_page', { title: 'Sign Up', alert: 'Udfyld alle felter' });
    }
    if (password !== confirmPassword) {
      return res.status(400).render('signup_page', { title: 'Sign Up', alert: 'Adgangskode og Gentag adgangskode skal være ens' });
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
      return res.status(400).render('signup_page', { title: 'Sign Up', alert: 'Brugernavnet er allerede i brug' });
    }

    const hashed = bcrypt.hashSync(password, 10);
    data.userdata.push({ username, password: hashed });
    fs.writeFileSync(jsonPath, JSON.stringify(data, null, 2), 'utf8');

    return res.status(201).render('Login_Page', { title: 'Login System', alert: 'Bruger oprettet. Log ind.' });
  } catch (err) {
    console.error('Fejl ved skrivning til Database:', err);
    return res.status(500).render('signup_page', { title: 'Sign Up', alert: 'Server fejl' });
  }
});

app.listen(port, () => {
  console.log(`Server is running on http://localhost:${port}`);
});