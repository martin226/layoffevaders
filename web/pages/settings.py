import streamlit as st
from utils import logo


# Sidebar operations
logo()

# difficulty (easy, medium, hard) cal/hour
# weight

st.title("Settings")

st.write("#### Workout Difficulty")

difficulty, weightsAdded = st.columns(border=True, spec=2, vertical_alignment="center")
difficulty.pills(options=["Light", "Moderate", "Intense"], label="Fitness Goal", default="Moderate")
weightsAdded.slider("Weights Added (lbs)", 0, 20, 0)

# Settings for jump height, lateral raise height, squat depth
st.write("#### Movement Calibration")

jumpHeight, lateralRaiseHeight, squatDepth = st.columns(3, vertical_alignment="center", border=True)

jumpHeight.slider("Jump Height (cm)", 0, 100, 30)
lateralRaiseHeight.slider("Lateral Raise Height (cm)", 0, 100, 50)
squatDepth.slider("Squat Depth (cm)", 0, 100, 30)