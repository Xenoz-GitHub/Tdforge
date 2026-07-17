import type { ReactNode } from 'react'
import { createContext, useContext, useState } from 'react'

interface GameContextType {
  isRunning: boolean
  isPaused: boolean
  mode: 'game' | 'console'
  start: () => void
  stop: () => void
  pause: () => void
  resume: () => void
  toggleMode: () => void
}

const GameContext = createContext<GameContextType | null>(null)

export function GameProvider({ children }: { children: ReactNode }) {
  const [isRunning, setIsRunning] = useState(false)
  const [isPaused, setIsPaused] = useState(false)
  const [mode, setMode] = useState<'game' | 'console'>('game')

  const start = () => { setIsRunning(true); setIsPaused(false) }
  const stop = () => { setIsRunning(false); setIsPaused(false) }
  const pause = () => setIsPaused(true)
  const resume = () => setIsPaused(false)
  const toggleMode = () => setMode(prev => prev === 'game' ? 'console' : 'game')

  return (
    <GameContext.Provider value={{ isRunning, isPaused, mode, start, stop, pause, resume, toggleMode }}>
      {children}
    </GameContext.Provider>
  )
}

export function useGame(): GameContextType {
  const ctx = useContext(GameContext)
  if (!ctx) throw new Error('useGame must be used within GameProvider')
  return ctx
}
