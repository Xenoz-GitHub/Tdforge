import { useRef, useEffect, useState } from 'react'
import { useGame } from '@/contexts/GameContext'

interface GamePreviewProps {
  width: number
  height: number
}

export function GamePreview({ width, height }: GamePreviewProps) {
  const canvasRef = useRef<HTMLCanvasElement>(null)
  const { isRunning } = useGame()
  const [, setReady] = useState(false)

  useEffect(() => {
    const canvas = canvasRef.current
    if (!canvas) return
    const ctx = canvas.getContext('2d')
    if (!ctx) return

    // Draw default checkerboard background
    const tileSize = 16
    const cols = Math.ceil(canvas.width / tileSize)
    const rows = Math.ceil(canvas.height / tileSize)
    for (let r = 0; r < rows; r++) {
      for (let c = 0; c < cols; c++) {
        ctx.fillStyle = (r + c) % 2 === 0 ? '#1a1020' : '#221430'
        ctx.fillRect(c * tileSize, r * tileSize, tileSize, tileSize)
      }
    }

    // Draw placeholder text
    ctx.fillStyle = '#443055'
    ctx.font = '14px monospace'
    ctx.textAlign = 'center'
    ctx.fillText(isRunning ? 'Game is running...' : 'Press Play to run', canvas.width / 2, canvas.height / 2)

    setReady(true)
  }, [isRunning, width, height])

  return (
    <canvas
      ref={canvasRef}
      width={width}
      height={height}
      style={{
        width: '100%',
        height: '100%',
        display: 'block',
        imageRendering: 'pixelated',
      }}
    />
  )
}
