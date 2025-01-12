from collections import defaultdict
from collections.abc import Callable
import json
import threading
import firebase_admin
from firebase_admin import credentials, db


# Initialize Firebase
cred = credentials.Certificate("service-account.json")
firebase_admin.initialize_app(cred, options={
    'databaseURL': "https://layoff-evaders-default-rtdb.firebaseio.com/",
})

class FirebaseManager:
    def __init__(self, user="user1"):
        self.user = user
        self.thread = None
        self.stop_event = threading.Event()
                
    def get_user_data(self):
        """Get a snapshot of the entire user's data from Firebase."""
        ref = db.reference(f"/users/{self.user}")
        return ref.get()
        
    def close(self):
        """Close the Firebase manager."""
        if self.stop_event and self.thread:
            self.stop_event.set()
            self.thread.join()
    
    def listen(self, path: str, callback: Callable):
        """Listen for changes in the user's data. path should NOT start with a slash. Will give you the value of the data at the path everytime it updates, in the correct datatype."""
        ref = db.reference(f"/users/{self.user}/{path}")
        
        def wrapper(event):
            data = self.get_user_data()
            
            print(data)
            callback(data)
            
        def run():
            self.stop_event.clear()
            ref.listen(wrapper)
        
        self.thread = threading.Thread(target=run, daemon=True)
        self.thread.start()
        

