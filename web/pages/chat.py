import types
import streamlit as st
from cohere import ClassifyExample
from utils import logo, generate_heat_map, generate_calories_burn_graph, generate_activity_count_graph, generate_calories_burnt, generate_exercise_counts, generate_session_length_graph, generate_sessions, generate_session_graph


# Sidebar operations
logo()


st.title("Chat")


# output history
for msg in st.session_state["Message"]:
    container = st.chat_message(msg["Name"])
    value = msg["Message"]
    # apparently container.write(*value) doesn't work
    for v in value:
        if isinstance(v, types.LambdaType):
            v(0)
        else:
            container.write(v)


def update_session(user, value):

    container = st.chat_message(user)

    # apparently container.write(*value) doesn't work
    for v in value:
        if isinstance(v, types.LambdaType):
            v(0)
        else:
            container.write(v)

    st.session_state["Message"].append({
        "Name": user,
        "Message": value
    })


prompt = st.chat_input("Message the Layoff Evaders AI!")

if prompt is not None:
    update_session("user", [prompt])

    # classify which type of response it is
    inputs = [prompt]
    examples = [
        # Total Exercise Count
        ClassifyExample(text="What is my total jump count?", label="Total Exercise Count"),
        ClassifyExample(text="Hey Layoff Evaders AI, how many squats did I do overall", label="Total Exercise Count"),
        # Not health related
        ClassifyExample(text="Whats the best way to get our VR working?", label="Unrelated to Health"),
        ClassifyExample(text="What's up Layoff Evaders bot?", label="Unrelated to Health"),
        # Health related query
        ClassifyExample(text="Hey there, whats the best way to exercise?", label="Health Related"),
        ClassifyExample(text="Whats the best way to clean up my diet?", label="Health Related"),
        ClassifyExample(text="Hi I'm 180 pounds and am 6 foot. What is my BMR?", label="Health Related"),
        # Total Calories Burned
        ClassifyExample(text="What is my total calories burned?", label="Total Calories Burned"),
        ClassifyExample(text="Can you show me the graphs on how many calories I burned?", label="Total Calories Burned"),
        # Session Workout Data
        ClassifyExample(text="Can you tell me about my workout sessions?", label="Session Workout Data"),
        ClassifyExample(text="How much long did I work out just now?", label="Session Workout Data"),
        # Heatmap
        ClassifyExample(text="Can you show me the exercise heatmap?", label="Heatmap"),
        ClassifyExample(text="I want to know how many days I have worked out for in a row", label="Heatmap")
    ]
    classification = st.session_state["Cohere"].classify(
        inputs=inputs,
        examples=examples
    ).classifications[0].prediction

    if classification == "Unrelated to Health":

        update_session("ai", ["Please ask the Layoff Evaders AI something related to your Health"])

    elif classification == "Health Related":

        response = st.session_state["Cohere"].chat(
            model="command-r", 
            messages=[{"role": "user", "content": prompt}],
            max_tokens=300
        )
        update_session("ai", [response.message.content[0].text])
    
    else:
        
        counts = st.session_state["Data"]["user1"]
        games = st.session_state["Data"]["user1"]["games"]
        latest_game = games[max(games.keys())]
        sessions = generate_sessions(games)

        if classification == "Total Exercise Count":

            update_session(
                "ai", 
                [
                    "Hi! Here are your total exercise counts, along with a detailed chart for each session",
                    lambda x: generate_exercise_counts(
                        st,
                        counts["jumpCount"],
                        counts["squatCount"],
                        counts["lateralRaiseCount"],
                        latest_game["jumpCount"],
                        latest_game["squatCount"],
                        latest_game["lateralRaiseCount"]
                    ),
                    lambda x: generate_activity_count_graph(st, sessions)
                ]
            )

        elif classification == "Total Calories Burned":

            update_session(
                "ai",
                [
                    "Here is the data you requested on the calories burned, accompanied by a chart showing the total for each session",
                    lambda x: generate_calories_burnt(
                        st, 
                        counts["jumpCount"],
                        counts["squatCount"],
                        counts["lateralRaiseCount"],
                        latest_game["jumpCount"],
                        latest_game["squatCount"],
                        latest_game["lateralRaiseCount"]
                    ),
                    lambda x: generate_calories_burn_graph(st, sessions)
                ]
            )

        elif classification == "Session Workout Data":

            update_session(
                "ai",
                [
                    "As requested, here is your session workout data along with a chart showing the duration of each session",
                    lambda x: generate_session_graph(st, sessions),
                    lambda x: generate_session_length_graph(st, sessions)
                ]
            )

        elif classification == "Heatmap":

            update_session(
                "ai", 
                [
                    "Here's the heat map you asked for, showing how frequently you've been working out lately",
                    lambda x: generate_heat_map(st, games)
                ]
            )
        
                