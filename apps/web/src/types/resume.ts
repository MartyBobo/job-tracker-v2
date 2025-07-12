export interface Resume {
  id: string
  title: string
  userId: string
  templateId?: string
  content: ResumeContent
  createdAt: Date
  updatedAt: Date
}

export interface ResumeContent {
  sections: ResumeSection[]
  settings: ResumeSettings
}

export interface ResumeSection {
  id: string
  type: SectionType
  position: Position
  size: Size
  content: any // Will be specific to each section type
  style?: SectionStyle
}

export type SectionType = 
  | 'contact'
  | 'experience'
  | 'education'
  | 'skills'
  | 'projects'
  | 'certifications'
  | 'summary'
  | 'custom'

export interface Position {
  x: number
  y: number
}

export interface Size {
  width: number
  height: number
}

export interface SectionStyle {
  backgroundColor?: string
  borderColor?: string
  borderWidth?: number
  borderRadius?: number
  padding?: number
  fontFamily?: string
  fontSize?: number
  color?: string
}

export interface ResumeSettings {
  pageSize: 'letter' | 'a4'
  margins: {
    top: number
    right: number
    bottom: number
    left: number
  }
  gridSize: number
  snapToGrid: boolean
  showGrid: boolean
}