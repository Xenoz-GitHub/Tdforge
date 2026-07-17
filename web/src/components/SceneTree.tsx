import type { SceneObject } from '@/contexts/SceneContext'

interface SceneTreeProps {
  objects: SceneObject[]
  selectedId: string | null
  onSelect: (id: string | null) => void
}

function TreeNode({ obj, selectedId, onSelect, depth }: { obj: SceneObject; selectedId: string | null; onSelect: (id: string | null) => void; depth: number }) {
  const isSelected = obj.id === selectedId
  return (
    <div>
      <div
        onClick={() => onSelect(obj.id)}
        style={{
          ...styles.node,
          ...(isSelected ? styles.nodeSelected : {}),
          paddingLeft: 12 + depth * 16,
        }}
      >
        <span style={styles.nodeIcon}>
          {obj.children.length > 0 ? (false ? '▾' : '▸') : ''}
        </span>
        <span style={styles.nodeName}>{obj.name}</span>
        <span style={styles.nodeType}>{obj.type}</span>
      </div>
      {obj.children.map(child => (
        <TreeNode key={child.id} obj={child} selectedId={selectedId} onSelect={onSelect} depth={depth + 1} />
      ))}
    </div>
  )
}

export function SceneTree({ objects, selectedId, onSelect }: SceneTreeProps) {
  if (objects.length === 0) {
    return <div style={styles.empty}>No objects</div>
  }

  return (
    <div style={styles.container}>
      {objects.map(obj => (
        <TreeNode key={obj.id} obj={obj} selectedId={selectedId} onSelect={onSelect} depth={0} />
      ))}
    </div>
  )
}

const styles: Record<string, React.CSSProperties> = {
  container: {
    padding: '2px 0',
    overflow: 'auto',
    flex: 1,
  },
  node: {
    display: 'flex',
    alignItems: 'center',
    gap: 4,
    padding: '2px 8px',
    cursor: 'pointer',
    fontSize: 12,
    color: '#e0d0f0',
  },
  nodeSelected: {
    background: '#3a2060',
  },
  nodeIcon: {
    width: 12,
    color: '#9880a8',
    fontSize: 10,
  },
  nodeName: {
    color: '#e0d0f0',
  },
  nodeType: {
    color: '#9880a8',
    fontSize: 10,
    marginLeft: 4,
  },
  empty: {
    padding: 20,
    color: '#443055',
    fontSize: 12,
    textAlign: 'center',
  },
}
