// Importe webserver, stier, filsystem og sessioner
const express = require('express');
const path = require('path');
const fs = require('fs');
const session = require('express-session');

// Opret Express app
const app = express();

// Port til serveren bruges 3000 som standard
const port = 3000;

// Konfigurer EJS på mappe med .ejs-filer
app.set('view engine', 'ejs');
app.set('views', path.join(__dirname, '/'));

// Middleware til at parse form data
app.use(express.urlencoded({ extended: true }));
app.use(express.json());

// Konfigurer sessioner gemmer et lille login-token i en cookie
app.use(
  session({
    secret: 'dev-secret', // nøgle til signering af session-cookies
    resave: false,        // gem ikke session hvis den ikke er ændret
    saveUninitialized: false, // opret ikke tomme sessioner
    cookie: { maxAge: 1000 * 60 * 60 }, // session varer 1 time
  })
);

// Forside login formular
app.get('/', (req, res) => {
  res.render('frontside', { title: 'Login System' });
});

// Vis signup side
app.get('/signup', (req, res) => {
  res.render('signup', { title: 'Sign Up' });
});

// Beskyttet side: kun synlig hvis man er logget ind ellers ser man forsiden
app.get('/login', (req, res) => {
  if (!req.session.user) return res.redirect('/');
  res.render('login', { title: 'Logget ind', username: req.session.user.username });
});

// Håndter login: læs brugere fra Databasen og sammenlign med inpute
app.post('/login', (req, res) => {
  const { username, password } = req.body || {};
  
  console.log('Login forsøg:', { username, password, hasBody: !!req.body });
  
  try {
    // Find og læs brugerdatabase (user.json)
    const jsonPath = path.join(__dirname, 'user', 'user.json');
    
    // Tjek om filen eksisterer
    if (!fs.existsSync(jsonPath)) {
      console.log('user.json filen findes ikke!');
      return res.status(401).render('frontside', { title: 'Login System', alert: 'Ingen brugere fundet. Opret en konto først.' });
    }
    
    const raw = fs.readFileSync(jsonPath, 'utf8');
    const data = JSON.parse(raw);
    console.log('Data fra JSON:', data);
    
    // Sørg for at have en liste af brugere
    const list = Array.isArray(data.userdata) ? data.userdata : (data.userdata ? [data.userdata] : []);
    console.log('Brugerliste:', list);

    // Find bruger med matching brugernavn
    const found = list.find(u => u.username === username);
    console.log('Fundet bruger:', found);
    console.log('Password sammenligning:', password, '===', found?.password, '=', password === found?.password);
    
    // Sammenlign indtastet kodeord direkte (plain text)
    if (found && password === found.password) {
      console.log('Login succesfuldt!');
      // Gem logged-in status i sessionen
      req.session.user = { username: found.username };
      return res.redirect('/login'); // vis den beskyttede side
    }
    console.log('Login fejlede - forkert brugernavn eller kodeord');
    // Forkert login: vis fejl på forsiden
    return res.status(401).render('frontside', { title: 'Login System', alert: 'Ugyldigt brugernavn eller adgangskode' });
  } catch (err) {
    // Hvis der er fejl ved læsning af databasen, log og send 500
    console.error('Fejl ved læsning af Databasen:', err);
    return res.status(500).send('Server fejl');
  }
});

// Log ud: ryd session og send tilbage til forsiden
app.post('/logout', (req, res) => {
  req.session.destroy(() => {
    res.redirect('/');
  });
});

// Håndter signup: valider felter, tjek dublet, hash kodeord og gem i Databasen
app.post('/signup', (req, res) => {
  const { username, password, confirmPassword } = req.body || {};
  try {
    // Simpel validering af input
    if (!username || !password || !confirmPassword) {
      return res.status(400).render('signup', { title: 'Sign Up', alert: 'Udfyld alle felter' });
    }
    // Tjek at adgangskode og bekræftelse matcher
    if (password !== confirmPassword) {
      return res.status(400).render('signup', { title: 'Sign Up', alert: 'Adgangskode og Gentag adgangskode skal være ens' });
    }

    // Indlæs eksisterende brugere fra Databasen
    const jsonPath = path.join(__dirname, 'user', 'user.json');
    let data = { userdata: [] };
    if (fs.existsSync(jsonPath)) {
      const raw = fs.readFileSync(jsonPath, 'utf8');
      data = JSON.parse(raw);
    }
    // Sørg for at have en liste af brugere
    if (!Array.isArray(data.userdata)) {
      data.userdata = data.userdata ? [data.userdata] : [];
    }

    // Tjek om brugernavn allerede findes
    const exists = data.userdata.some(u => u.username === username);
    if (exists) {
      return res.status(400).render('signup', { title: 'Sign Up', alert: 'Brugernavnet er allerede i brug' });
    }

    // Gem ny bruger med plain text kodeord
    data.userdata.push({ username, password });
    fs.writeFileSync(jsonPath, JSON.stringify(data, null, 2), 'utf8');

    // Succes: tilbage til login med besked
    return res.status(201).render('frontside', { title: 'Login System', alert: 'Bruger oprettet. Log ind.' });
  } catch (err) {
    // Fejl ved skrivning/parsing: vis generel fejl
    console.error('Fejl ved skrivning til Database:', err);
    return res.status(500).render('signup', { title: 'Sign Up', alert: 'Server fejl' });
  }
});

// Start HTTP-serveren
app.listen(port, () => {
  console.log(`Server is running on http://localhost:${port}`);
});