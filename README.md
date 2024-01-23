# Gridworld
###### .NET 6

#### Pseudocode
<pre>
Initialize <i>Q(s,a)</i> arbitrarily and <i>e(s,a) = 0</i>, for all <i>s,a</i>
Repeat (for each episode):
	Initialize <i>s,a</i>
	Repeat (for each step of episode):
		Take action <i>a</i>, observe <i>r,s’</i>
		Choose <i>a’</i> from <i>s’</i> using policy derived from Q (e.g., <i>ϵ</i>-greedy)
		<i>δ ← r + γ Q(s’,a’) – Q(s,a)</i>
		<i>e(s,a) ← e(s,a) + 1</i>
		For all <i>s,a</i>:
			<i>Q(s,a) ← Q(s,a) + α δ e(s,a)</i>
			<i>e(s,a) ← γ λ e(s,a)</i>
		<i>s ← s’ ; a ← a’</i>
	until <i>s</i> is terminal
</pre>

#### Description
An AI agent learns the optimal path towards its goal from any starting point using SARSA-λ.
The algorithm works by initializing a 2D grid with each square containing 4 arbitrarily small weights representing UP, DOWN, LEFT, RIGHT directions. Our gamma discount factor begins at 0.90 and our lambda value is static with the value 0.00000005. We then decide to either explore or exploit within our given state Q(s,a) by stochastically selecting from decaying odds`(tmp < gamma) ? explore() : exploit()`. For each episode until we reach a terminal state we take an action (decided earlier using explore/exploit), to observe the reward in the next state, s'. Then we choose a next action a' from the next state s' using a greedy policy derived from Q. In other words we make our next move based on the maximum value of the state s'. We then calculate a delta by factoring in the appropriate reward value (gathered from s') added to the gamma discount factor multiplied by the difference of Q(s',a') - Q(s,a). We then update the corresponding eligibility value for the weight in Q(s,a). Now we update our entire gridworld table. First we update all the weights by taking each weight in Q(s,a) and adding to it our learning alpha multiplied by the delta we calculated earlier multiplied by the corresponding eligibility value e(s,a). Secondly, we update our eligibility values by multiplying each eligibility value by gamma multiplied lambda. The last step is to now move our AI agent to the next state Q(s',a'). Keep in mind, Q(s',a') was determined early on using either exploration or exploitation.

We keep performing the above algorithm, until state s' reaches a terminal state, delinated by state s being the goal location, out of bounds, or an obstacle. This terminal state will be checked after we move s <- s' ; a <- a'. If we hit a terminal state, we update gamma by subtracting lambda from it (gamma -= lambda). The purpose of this allows our AI agent early on to explore and gather more data about its environment. As the algorithm progresses, the AI learns more about its environment and as such its actions will reflect that by exploiting on what it has learned (the newly updated weights).
