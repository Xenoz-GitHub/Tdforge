import { useEffect, useRef, useCallback } from 'react'

interface GameLoopState {
  isRunning: boolean
  isPaused: boolean
  start: () => void
  stop: () => void
  pause: () => void
  resume: () => void
}

export function useGameLoop(
  canvasRef: React.RefObject<HTMLCanvasElement | null>,
  onFrame: (dt: number, ctx: CanvasRenderingContext2D) => void
): GameLoopState {
  const rafId = useRef<number>(0)
  const lastTime = useRef<number>(0)
  const running = useRef(false)
  const paused = useRef(false)
  const frameFn = useRef(onFrame)
  frameFn.current = onFrame

  const loop = useCallback((time: number) => {
    if (!running.current) return
    if (!paused.current) {
      const dt = lastTime.current ? (time - lastTime.current) / 1000 : 1 / 60
      lastTime.current = time
      const canvas = canvasRef.current
      if (canvas) {
        const ctx = canvas.getContext('2d')
        if (ctx) {
          frameFn.current(dt, ctx)
        }
      }
    }
    rafId.current = requestAnimationFrame(loop)
  }, [canvasRef])

  const start = useCallback(() => {
    if (running.current) return
    running.current = true
    paused.current = false
    lastTime.current = 0
    rafId.current = requestAnimationFrame(loop)
  }, [loop])

  const stop = useCallback(() => {
    running.current = false
    paused.current = false
    cancelAnimationFrame(rafId.current)
  }, [])

  const pause = useCallback(() => {
    paused.current = true
  }, [])

  const resume = useCallback(() => {
    paused.current = false
    lastTime.current = 0
  }, [])

  useEffect(() => {
    return () => {
      running.current = false
      cancelAnimationFrame(rafId.current)
    }
  }, [])

  return { isRunning: running.current, isPaused: paused.current, start, stop, pause, resume }
}
