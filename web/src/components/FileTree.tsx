import { Icon } from '@/components/Icon'

interface FileItem {
  name: string
  path: string
  isDirectory?: boolean
  children?: FileItem[]
}

interface FileTreeProps {
  files: FileItem[]
  onOpenFile: (path: string) => void
  activeFile?: string
}

export function FileTree({ files, onOpenFile, activeFile }: FileTreeProps) {
  if (files.length === 0) {
    return (
      <div style={styles.empty}>
        <span>No files</span>
        <div style={{ fontSize: 10, color: '#443055', marginTop: 4 }}>
          Open a project to get started
        </div>
      </div>
    )
  }

  return (
    <div style={styles.container}>
      {files.map(file => (
        <div
          key={file.path}
          onClick={() => !file.isDirectory && onOpenFile(file.path)}
          style={{
            ...styles.item,
            ...(activeFile === file.path ? styles.itemActive : {}),
            ...(file.isDirectory ? styles.itemDir : {}),
          }}
        >
          <Icon name={file.isDirectory ? 'Folder' : 'File'} size={14} />
          <span style={styles.name}>{file.name}</span>
        </div>
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
  item: {
    display: 'flex',
    alignItems: 'center',
    gap: 6,
    padding: '3px 8px',
    cursor: 'pointer',
    fontSize: 12,
    color: '#e0d0f0',
  },
  itemActive: {
    background: '#3a2060',
  },
  itemDir: {
    fontWeight: 600,
    color: '#c0b0d0',
  },
  name: {
    overflow: 'hidden',
    textOverflow: 'ellipsis',
    whiteSpace: 'nowrap',
  },
  empty: {
    padding: 20,
    color: '#443055',
    fontSize: 12,
    textAlign: 'center',
  },
}
