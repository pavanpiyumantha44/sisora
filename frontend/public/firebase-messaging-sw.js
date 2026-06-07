importScripts('https://www.gstatic.com/firebasejs/10.12.0/firebase-app-compat.js');
importScripts('https://www.gstatic.com/firebasejs/10.12.0/firebase-messaging-compat.js');

// paste your firebase config here directly
// service workers can't access import.meta.env
firebase.initializeApp({
  apiKey: "AIzaSyCd55j-RLzF390f2cXYrV1t0LYrnr7LNks",
  authDomain: "sisora-a001b.firebaseapp.com",
  projectId: "sisora-a001b",
  storageBucket: "sisora-a001b.firebasestorage.app",
  messagingSenderId: "563310503784",
  appId: "1:563310503784:web:30b1566ab22ab7412058dd"
});

const messaging = firebase.messaging();

messaging.onBackgroundMessage((payload) => {
  const { title, body } = payload.notification;

  self.registration.showNotification(title, {
    body,
    icon: '/icon-192x192.png'
  });
});