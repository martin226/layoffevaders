import streamlit as st
import cohere
import pytz
from secret import API_KEY
from firebase_manager import FirebaseManager
from time import sleep
from datetime import datetime
from utils import logo, generate_heat_map, generate_activity_count_graph, generate_calories_burn_graph, generate_calories_burnt, generate_exercise_counts, generate_session_graph, generate_session_length_graph, generate_sessions


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

# init database

currentUser = "user1"


def parse_data(unparsed_data):

    game_keys = unparsed_data[currentUser]["games"].keys()
    for nxt_game in game_keys:
        est = pytz.timezone('US/Eastern')
        est_startTime = datetime.fromisoformat(unparsed_data[currentUser]["games"][nxt_game]["startTime"]).astimezone(est)
        est_endTime = datetime.fromisoformat(unparsed_data[currentUser]["games"][nxt_game]["endTime"]).astimezone(est)
        unparsed_data[currentUser]["games"][nxt_game]["startTime"] = est_startTime
        unparsed_data[currentUser]["games"][nxt_game]["endTime"] = est_endTime
    return unparsed_data


def callback(d):
    global c
    st.session_state["Data"] = parse_data(d)
    print(st.session_state["Data"]["user1"]["lateralRaiseCount"])
    #st.cache_data.clear()
    st.rerun()
    #st.write(st.session_state["Data"]["user1"]["lateralRaiseCount"])


if st.session_state == {}:

    st.session_state["Database"] = FirebaseManager()
    st.session_state["Cohere"] = cohere.ClientV2(API_KEY)
    st.session_state["Message"] = [
        {
            "Name": "ai",
            "Message": [(
                "Welcome to Layoff Evaders AI! 👋👋👋 Here is a list of commands to get you started \n\n"
                " → What is my total jump count?\n\n"
                " → What does my exercise heatmap look like?\n\n"
                " → Show me the chart for calories burned"
            )]
        }
    ]

st.session_state["Data"] = parse_data(st.session_state["Database"].get_user_data())

data = st.session_state["Data"]


st.title("Dashboard")
#st.markdown("#####") # adds vertical spacing

# total calories burnt
st.write("##### User Total Metrics")

latest_game = data[currentUser]["games"][max(data[currentUser]["games"].keys())]
generate_calories_burnt(
    st,
    data[currentUser]["jumpCount"],
    data[currentUser]["squatCount"],
    data[currentUser]["lateralRaiseCount"],
    latest_game["jumpCount"],
    latest_game["squatCount"],
    latest_game["lateralRaiseCount"]
)


st.write("##### Recent Sessions")
generate_exercise_counts(
    st,
    data[currentUser]["jumpCount"],
    data[currentUser]["squatCount"],
    data[currentUser]["lateralRaiseCount"],
    latest_game["jumpCount"],
    latest_game["squatCount"],
    latest_game["lateralRaiseCount"]
)


sessions = generate_sessions(data[currentUser]["games"])
generate_session_graph(st, sessions)
session_count = min(len(sessions), 10)

# create session graphs
st.markdown("##### Session Statistics")

chartDisplay = st.selectbox(
    label="no label",
    label_visibility="collapsed",
    options=["Session Length", "Activity Count", "Calories Burned"],
    index=0,
)


if chartDisplay == "Session Length":

    generate_session_length_graph(st, sessions)

elif chartDisplay == "Activity Count":

    generate_activity_count_graph(st, sessions)

elif chartDisplay == "Calories Burned":

    generate_calories_burn_graph(st, sessions)


# implement Github-like heat map
st.markdown("##### Heatmap")


generate_heat_map(st, data[currentUser]["games"])


# refresh page
sleep(3)
st.rerun()