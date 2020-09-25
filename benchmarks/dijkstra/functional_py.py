from collections import namedtuple
from functools import reduce
from pyrsistent import pmap, pdeque

edges = [("a", "b", 7),  ("a", "c", 9),
         ("a", "f", 14), ("b", "c", 10),
         ("b", "d", 15), ("c", "d", 11),
         ("c", "f", 2),  ("d", "e", 6),
         ("e", "f", 9)]

def listToQueue(moves):
    sortedList = sorted(moves,key=lambda x: x[2])
    return pdeque(sortedList)

def mergeQueues(q1,q2,res):
    if not q2:
        return res.extend(q1)
    elif not q1:
        return res.extend(q2)
    else: 
        if q1.left[2] < q2.left[2]:
            newRes = res.append(q1.left)
            newq1 = q1.popleft()
            return mergeQueues(newq1,q2,newRes)
        else:
            newRes = res.append(q2.left)
            newq2 = q2.popleft()
            return mergeQueues(q1,newq2,newRes)

def getNewMoves(edges, backtrackMap, pos, cost):
    # Filters all edges that are either the current pos or have already been visited
    moves = list(filter(lambda move: 
                            move[0] == pos and 
                            move[1] not in backtrackMap.keys(), 
                            edges))

    # Adds the cost of taking an edge
    return listToQueue(list(map(lambda move: (move[0], move[1], move[2] + cost), moves)))

def backtrack(backtrackMap, position, result):
    if position == '0':
        return result
    else:
        return backtrack(backtrackMap, backtrackMap[position], [position] + result)

def move(legalMovesQueue, backtrackMap, dest):
    nextMove = legalMovesQueue.left
    if nextMove[1] == dest:
        return backtrack(backtrackMap,nextMove[0],[nextMove[1]])
    else:
        newBacktrackMap = backtrackMap.update({nextMove[1]:nextMove[0]})
        oldMoves = legalMovesQueue.popleft()
        newMoves = mergeQueues(oldMoves, getNewMoves(edges,newBacktrackMap,nextMove[1],nextMove[2]),pdeque())
        return move(newMoves, newBacktrackMap, dest)


def shortestPath(edges, start, dest):
    moves = getNewMoves(edges, pmap({'a':'0'}), start, 0)
    return move(moves, pmap({'a':'0'}), dest)

shortestPath(edges, "a", "e")
