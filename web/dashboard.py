import pandas as pd
import numpy as np
import streamlit as st
import plotly.graph_objects as go
from datetime import date, datetime, timedelta
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
        "jumpCount": 32,
        "lateralRaiseCount": 31,
        "squatCount": 49,
        "games": [
            {
                "timeStart": datetime(2024, 5, 17, 14, 15, 14),
                "timeEnd": datetime(2024, 5, 17, 14, 41, 24),
                "jumpCount": 8,
                "lateralRaiseCount": 9,
                "squatCount": 17
            },
            {
                "timeStart": datetime(2024, 5, 17, 15, 7, 10),
                "timeEnd": datetime(2024, 5, 17, 16, 4, 9),
                "jumpCount": 15,
                "lateralRaiseCount": 7,
                "squatCount": 11
            },
            {
                "timeStart": datetime(2024, 5, 21, 13, 44, 3),
                "timeEnd": datetime(2024, 5, 21, 14, 15, 3),
                "jumpCount": 9,
                "lateralRaiseCount": 15,
                "squatCount": 21
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

    def get_length(self):

        return round((self.timeEnd - self.timeStart).total_seconds() / 60)

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

    length = s.get_length()

    c1.write(s.timeStart)
    c2.markdown(f"⌛ **{length}** min")
    c3.markdown(f"Jumps: **{s.jumps}**")
    c4.markdown(f"Squats: **{s.squats}**")
    c5.markdown(f"Lats: **{s.lateralRaises}**")
    c6.markdown(f"🔥 **{s.get_calorie_count()}** Cals")

# create session graphs
st.markdown("##### Session Statistics")
chartDisplay = st.selectbox(
    label="no label",
    label_visibility="collapsed",
    options=["Session Length", "Activity Count", "Calories Burned"],
    index=0,
)

recent_sessions = sessions[-session_count:]
if chartDisplay == "Session Length":

    df = pd.DataFrame({
        "Session Start": [s.timeStart for s in recent_sessions],
        "Session Length": [s.get_length() for s in recent_sessions]
    }).set_index("Session Start")
    st.line_chart(df, height=350)

elif chartDisplay == "Activity Count":

    df = pd.DataFrame({
        "Session Start": [s.timeStart for s in recent_sessions],
        "Session Jumps": [s.jumps for s in recent_sessions],
        "Session Squats": [s.squats for s in recent_sessions],
        #"Session Lateral Raises": [s.lateralRaises for s in recent_sessions]
    }).set_index("Session Start")
    st.bar_chart(df, height=350, stack=True)

elif chartDisplay == "Calories Burned":

    df = pd.DataFrame({
        "Session Start": [s.timeStart for s in recent_sessions],
        "Calories Burned": [s.get_calorie_count() for s in recent_sessions]
    }).set_index("Session Start")
    st.line_chart(df, height=350)


# implement Github-like heat map
st.markdown("##### Heatmap")

# find number of games played on each day
heatmap_counts = dict()
for nxt_game in data[currentUser]["games"]:
    d = str(nxt_game["timeStart"].date())
    if d in heatmap_counts:
        heatmap_counts[d] += 1
    else:
        heatmap_counts[d] = 1

# make 52 x 7
heatmap_values = [[] for _ in range(7)]
for i in range(364):
    # i days before current date
    old_date = str((datetime.today() - timedelta(days=i)).date())
    v = 0
    if old_date in heatmap_counts:
        v = heatmap_counts[old_date]
    heatmap_values[i % 7].append(v)

fig = go.Figure(
    data = go.Heatmap(
        z = heatmap_values,
        hoverinfo="skip",
        xgap=1,
        ygap=1,
        colorscale='Greens'
    ),
    layout = {
        'xaxis': {'visible': False, 'showticklabels': False},
        'yaxis': {'visible': False, 'showticklabels': False},
    },
)
fig.update_layout(yaxis_scaleanchor="x")
fig.update_traces(colorbar_orientation='h', selector=dict(type='heatmap'))
st.plotly_chart(fig)
#st.button(label=" ", type="primary")
#st.button(label=" ", type="secondary")