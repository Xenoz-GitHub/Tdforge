import { useScene } from '@/contexts/SceneContext'

interface StatusBarProps {
  tool: string
  mousePos: string
  zoom: string
}

export function StatusBar({ tool, mousePos, zoom }: StatusBarProps) {
  const { objects, selectedId } = useScene()
  const selection = selectedId ? objects.find(o => o.id === selectedId)?.name ?? '' : ''
  const objCount = objects.length

  return (
    <div style={styles.bar}>
      <span style={styles.tool}>{tool}</span>
      <span style={styles.spacer} />
      <span style={styles.item}>{mousePos}</span>
      <span style={styles.item}>{zoom}</span>
      <span style={styles.item}>{selection}</span>
      <span style={styles.item}>{objCount > 0 ? `${objCount} objects` : 'Empty scene'}</span>
    </div>
  )
}

const styles: Record<string, React.CSSProperties> = {
  bar: {
    height: 24,
    background: '#1a1020',
    display: 'flex',
    alignItems: 'center',
    padding: '0 8px',
    borderTop: '1px solid #332044',
    fontSize: 10,
    gap: 12,
  },
  tool: {
    color: '#8b4fc8',
    fontWeight: 700,
    fontSize: 11,
  },
  spacer: {
    flex: 1,
  },
  item: {
    color: '#9880a8',
  },
}
