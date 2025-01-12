import pandas as pd
import streamlit as st
import plotly.graph_objects as go
from streamlit_extras.app_logo import add_logo
from datetime import datetime, timedelta


@st.cache_data
def logo():
    
    add_logo("cow.jpeg", height=450)


def calories_burnt(jumps, squats, lats):

    squat_MET = 3
    jump_MET = 4
    lat_MET = 2

    return round(squat_MET * 1/3600 * squats + jump_MET * 1/3600 * jumps + lat_MET * 1/3600 * lats * 60, 1)

# calories burnt = MET x weight (kg) x duration (hours)
# assumption: one action is 1 second -> 1/3600 hours
# assumption: person weighs 60kg
def generate_calories_burnt(c, jump, squat, lat, lJump, lSquat, lLat):

    caloriesBurnt = calories_burnt(jump, squat, lat)
    caloriesBurntLatestGame = calories_burnt(lJump, lSquat, lLat)
    c.metric(label="Total Calories Burnt", value=caloriesBurnt, delta=caloriesBurntLatestGame, delta_color="normal", border=True)


def generate_exercise_counts(c, jump, squat, lat, lJump, lSquat, lLat):

    totalJumps, totalLateralRaises, totalSquats = c.columns(spec=3, gap="small", vertical_alignment="center", border=True)

    # total count of each action
    totalJumps.metric(label="Jump Count", value=jump, delta=lJump, delta_color="normal")
    totalLateralRaises.metric(label="Lateral Raise Count", value=lat, delta=lLat, delta_color="normal")
    totalSquats.metric(label="Squat Count", value=squat, delta=lSquat, delta_color="normal")


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
def generate_sessions(games):

    sessions = []
    current_session = None
    sorted_key_set = sorted(games.keys())
    for next_key in sorted_key_set:

        next_game = games[next_key]

        if current_session is None:
        
            current_session = Session(
                next_game["startTime"],
                next_game["endTime"],
                next_game["jumpCount"],
                next_game["squatCount"],
                next_game["lateralRaiseCount"]
            )

        elif (next_game["startTime"] - current_session.timeEnd).total_seconds() / 60 <= 60:# add to current session
            current_session.jumps += next_game["jumpCount"]
            current_session.squats += next_game["squatCount"]
            current_session.lateralRaises += next_game["lateralRaiseCount"]
            current_session.timeEnd = next_game["endTime"]
    
        else: # create a new session
            sessions.append(current_session)
            current_session = Session(
                next_game["startTime"],
                next_game["endTime"],
                next_game["jumpCount"],
                next_game["squatCount"],
                next_game["lateralRaiseCount"]
            )

    if current_session is not None:
        sessions.append(current_session)
    
    return sessions


def generate_session_graph(c, sessions):

    session_count = min(len(sessions), 10)
    session_container = c.container(height=225, border=True)

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
        c6.markdown(f"🔥 **{s.get_calorie_count()}**")


def generate_session_length_graph(c, sessions):
    
    session_count = min(len(sessions), 10)
    recent_sessions = sessions[-session_count:]
    df = pd.DataFrame({
        "Session Start": [s.timeStart for s in recent_sessions],
        "Session Length": [s.get_length() for s in recent_sessions]
    }).set_index("Session Start")
    c.line_chart(df, height=350)


def generate_activity_count_graph(c, sessions):
    
    session_count = min(len(sessions), 10)
    recent_sessions = sessions[-session_count:]
    df = pd.DataFrame({
        "Session Start": [s.timeStart for s in recent_sessions],
        "Session Jumps": [s.jumps for s in recent_sessions],
        "Session Squats": [s.squats for s in recent_sessions],
        "Session Lateral Raises": [s.lateralRaises for s in recent_sessions]
    }).set_index("Session Start")
    c.bar_chart(df, height=350, stack=True)


def generate_calories_burn_graph(c, sessions):

    session_count = min(len(sessions), 10)
    recent_sessions = sessions[-session_count:]
    df = pd.DataFrame({
        "Session Start": [s.timeStart for s in recent_sessions],
        "Calories Burned": [s.get_calorie_count() for s in recent_sessions]
    }).set_index("Session Start")
    c.line_chart(df, height=350)


def generate_session_length_graph(c, sessions):

    session_count = min(len(sessions), 10)
    recent_sessions = sessions[-session_count:]
    df = pd.DataFrame({
        "Session Start": [s.timeStart for s in recent_sessions],
        "Session Length": [s.get_length() for s in recent_sessions]
    }).set_index("Session Start")
    c.line_chart(df, height=350)


def generate_activity_count_graph(c, sessions):
    
    session_count = min(len(sessions), 10)
    recent_sessions = sessions[-session_count:]
    df = pd.DataFrame({
        "Session Start": [s.timeStart for s in recent_sessions],
        "Session Jumps": [s.jumps for s in recent_sessions],
        "Session Squats": [s.squats for s in recent_sessions],
        "Session Lateral Raises": [s.lateralRaises for s in recent_sessions]
    }).set_index("Session Start")
    c.bar_chart(df, height=350, stack=True)


def generate_calories_burn_graph(c, sessions):

    session_count = min(len(sessions), 10)
    recent_sessions = sessions[-session_count:]
    df = pd.DataFrame({
        "Session Start": [s.timeStart for s in recent_sessions],
        "Calories Burned": [s.get_calorie_count() for s in recent_sessions]
    }).set_index("Session Start")
    c.line_chart(df, height=350)


def generate_heat_map(c, games):

    # find number of games played on each day
    heatmap_counts = dict()
    game_keys = games.keys()
    for nxt_key in game_keys:
        nxt_game = games[nxt_key]
        d = str(nxt_game["startTime"].date())
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

    for i in range(7):
        heatmap_values[i] = heatmap_values[i][::-1]

    fig = go.Figure(
        data = go.Heatmap(
            z = heatmap_values,
            hoverinfo="skip",
            xgap=1,
            ygap=1,
            colorscale=[[0, 'white'], [1, 'green']]
        ),
        layout = {
            'xaxis': {'visible': False, 'showticklabels': False},
            'yaxis': {'visible': False, 'showticklabels': False},
        },
    )

    fig.update_layout(yaxis_scaleanchor="x")
    fig.update_traces(colorbar_orientation='h', selector=dict(type='heatmap'))
    c.plotly_chart(fig, key="heatmap")