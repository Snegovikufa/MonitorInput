# MonitorController

This app helps you to share one monitor between two PCs. 

# Usage

    MonitorInputController.exe [inputID]

### If you don't know *magic* inputID numbers

you need to run app without arguments and test monitor input switch by your own. Usually you need to check range [0, 15]

### If you already know your *magic* input ID numbers

	# For example you want to switch monitor input to DisplayPort:
	MonitorInputController.exe 15
	# or into DVI:
	MonitorInputController.exe 3
