import pandas as pd
import numpy as np
import streamlit as st
from datetime import date, datetime
from streamlit_extras.app_logo import add_logo
from utils import logo


# Sidebar operations
logo()


#add_logo
#- needs to be on every single page
#- size is based on image size
#- height controlls how low the navigation bar is

# users
# - user1
# - - jumpCount
# - - lateralRaiseCount
# - - squatCount

# - - games
# - - - time period (date time)
# - - jumpCount, lateralRaiseCount, squatCount
# one session is 60 minutes

data = {
    "user1": {
        "jumpCount": 5,
        "lateralRaiseCount": 6,
        "squatCount": 9,
        "games": [
            {
                "timeStart": datetime(2020, 5, 17),
                "timeEnd": datetime(2020, 5, 17),
                "jumpCount": 2,
                "lateralRaiseCount": 4,
                "squatCount": 3
            },
            {
                "timeStart": datetime(2020, 5, 17),
                "timeEnd": datetime(2020, 5, 17),
                "jumpCount": 3,
                "lateralRaiseCount": 2,
                "squatCount": 6
            },
            {
                "timeStart": datetime(2020, 6, 17),
                "timeEnd": datetime(2020, 6, 17),
                "jumpCount": 9,
                "lateralRaiseCount": 1,
                "squatCount": 2
            }
        ]
    }
}

currentUser = "user1"

st.title("Dashboard")
#st.markdown("#####") # adds vertical spacing

def calories_burnt(jumps, squats, lats):

    squat_MET = 3
    jump_MET = 4
    lat_MET = 2

    return round(squat_MET * 1/3600 * squats + jump_MET * 1/3600 * jumps + lat_MET * 1/3600 * lats * 60, 1)

# calories burnt = MET x weight (kg) x duration (hours)
# assumption: one action is 1 second -> 1/3600 hours
# assumption: person weighs 60kg
caloriesBurnt = calories_burnt(data[currentUser]["jumpCount"], data[currentUser]["squatCount"], data[currentUser]["lateralRaiseCount"])

latest_game = data[currentUser]["games"][-1]
caloriesBurntLatestGame = calories_burnt(latest_game["jumpCount"], latest_game["squatCount"], latest_game["lateralRaiseCount"])

# total calories burnt
st.write("##### User Total Metrics")
st.metric(label="Total Calories Burnt", value=caloriesBurnt, delta=caloriesBurntLatestGame, delta_color="normal", border=True)

totalJumps, totalLateralRaises, totalSquats = st.columns(spec=3, gap="small", vertical_alignment="center", border=True)

# total count of each action
totalJumps.metric(label="Jump Count", value=data[currentUser]["jumpCount"], delta=latest_game["jumpCount"], delta_color="normal")
totalLateralRaises.metric(label="Lateral Raise Count", value=data[currentUser]["lateralRaiseCount"], delta=latest_game["lateralRaiseCount"], delta_color="normal")
totalSquats.metric(label="Squat Count", value=data[currentUser]["squatCount"], delta=latest_game["squatCount"], delta_color="normal")

st.write("##### Recent Sessions")

class Session:

    def __init__(self, timeStart, timeEnd, jumps, squats, lateralRaises):

        self.timeStart = timeStart
        self.timeEnd = timeEnd
        self.jumps = jumps
        self.squats = squats
        self.lateralRaises = lateralRaises
    
    def get_calorie_count(self):

        return calories_burnt(self.jumps, self.squats, self.lateralRaises)

# algorithm to divide into sessions
sessions = []
current_session = None
for next_game in data[currentUser]["games"]:

    if current_session is None:
        
        current_session = Session(
            next_game["timeStart"],
            next_game["timeEnd"],
            next_game["jumpCount"],
            next_game["squatCount"],
            next_game["lateralRaiseCount"]
        )

    elif (next_game["timeStart"] - current_session.timeEnd).total_seconds() / 60 <= 60:# add to current session
        current_session.jumps += next_game["jumpCount"]
        current_session.squats += next_game["squatCount"]
        current_session.lateralRaises += next_game["lateralRaiseCount"]
        current_session.timeEnd = next_game["timeEnd"]
    
    else: # create a new session
        sessions.append(current_session)
        current_session = Session(
            next_game["timeStart"],
            next_game["timeEnd"],
            next_game["jumpCount"],
            next_game["squatCount"],
            next_game["lateralRaiseCount"]
        )

if current_session is not None:
    sessions.append(current_session)

session_count = min(len(sessions), 10)
session_container = st.container(height=225, border=True)

for i in range(session_count):

    s = sessions[-i]
    container = session_container.container(border=True)
    c1, c2, c3, c4, c5, c6 = container.columns(spec=[2, 1, 1, 1, 1, 1], gap="small")

    length = round((s.timeEnd - s.timeStart).total_seconds() / 60)

    c1.write(s.timeStart)
    c2.markdown(f"⌛ **{length}** min")
    c3.markdown(f"Jumps: **{s.jumps}**")
    c4.markdown(f"Squats: **{s.squats}**")
    c5.markdown(f"Lats: **{s.lateralRaises}**")
    c6.markdown(f"🔥 **{s.get_calorie_count()}** Cals")
