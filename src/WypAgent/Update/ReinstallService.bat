sc stop WakeYourPC
sc delete WakeYourPC
sc create WakeYourPC binPath="%ProgramFiles(X86)%\TeamGreen\Wake Your PC\WakeYourPCAgent.exe"
sc start WakeYourPC
